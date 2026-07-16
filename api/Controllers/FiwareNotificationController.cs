using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Drivolution.Controllers;

// Recebe notificações do Orion Context Broker quando um skid muda de workstation.
// O Orion chama este endpoint automaticamente via subscrição NGSI-LD.
[ApiController]

// Define a rota base: /api/FiwareNotification
[Route("api/[controller]")]
public class FiwareNotificationController : ControllerBase
{
    // Repository responsável pelos suportes (skids)
    private readonly ISupportRepository _supportRepo;

    // Repository responsável pelas workstations
    private readonly IWorkstationRepository _workstationRepo;

    // Service responsável pelo histórico de localizações
    private readonly ILocalizationHistoryService _localizationService;

    // Service responsável pelas fases de produção dos produtos
    private readonly IProductPhaseService _productPhaseService;

    // Repository que relaciona skids com produtos
    private readonly ISupportedProductRepository _supportedProductRepo;

    // Repository para consultar fases dos produtos
    private readonly IProductPhaseRepository _productPhaseRepo;

    // Repository responsável pela sequência de fabrico dos modelos
    private readonly IPhaseSequenceRepository _phaseSequenceRepo;

    // Repository dos produtos
    private readonly IProductRepository _productRepo;

    // Repository dos alertas
    private readonly IAlertRepository _alertRepo;

    // Service responsável pela criação e resolução de alertas
    private readonly IAlertService _alertService;

    // Service responsável pela gestão da presença dos operadores
    private readonly IWorkstationPresenceService _presenceService;

    // Logger utilizado para registar informação e erros
    private readonly ILogger<FiwareNotificationController> _logger;

    // O ASP.NET injeta automaticamente todos os repositories e services necessários
    public FiwareNotificationController(
        ISupportRepository supportRepo,
        IWorkstationRepository workstationRepo,
        ILocalizationHistoryService localizationService,
        IProductPhaseService productPhaseService,
        ISupportedProductRepository supportedProductRepo,
        IProductPhaseRepository productPhaseRepo,
        IPhaseSequenceRepository phaseSequenceRepo,
        IProductRepository productRepo,
        IAlertRepository alertRepo,
        IAlertService alertService,
        IWorkstationPresenceService presenceService,
        ILogger<FiwareNotificationController> logger)
    {
        _supportRepo          = supportRepo;
        _workstationRepo      = workstationRepo;
        _localizationService  = localizationService;
        _productPhaseService  = productPhaseService;
        _supportedProductRepo = supportedProductRepo;
        _productPhaseRepo     = productPhaseRepo;
        _phaseSequenceRepo    = phaseSequenceRepo;
        _productRepo          = productRepo;
        _alertRepo            = alertRepo;
        _alertService         = alertService;
        _presenceService      = presenceService;
        _logger               = logger;
    }

    // Endpoint chamado automaticamente pelo Orion sempre que existe
    // uma alteração numa entidade subscrita (Skid ou Badge)
    [HttpPost]
    public async Task<IActionResult> Notify([FromBody] JsonElement body)
    {
        try
        {
            // Regista no log o conteúdo completo da notificação recebida
            _logger.LogInformation("Notificação FIWARE recebida: {Body}", body.ToString());

            // Verifica se existe o campo "data" na notificação
            if (!body.TryGetProperty("data", out var dataArray))
            {
                _logger.LogWarning("Notificação sem campo 'data'.");
                return Ok();
            }

            // Processa cada entidade enviada pelo Orion
            foreach (var entity in dataArray.EnumerateArray())
            {
                // Obtém o tipo da entidade (Skid ou Badge)
                var entityType = entity.TryGetProperty("type", out var typeProp)
                    ? typeProp.GetString()
                    : null;

                switch (entityType)
                {
                    // Evento proveniente de um crachá RFID
                    case "Badge":
                        await ProcessBadgeEvent(entity);
                        break;

                    // Evento proveniente de um skid RFID
                    case "Skid":
                    default:
                        // Notificações antigas/sem 'type' assumem Skid, para compatibilidade.
                        await ProcessSkidEvent(entity);
                        break;
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            // Regista qualquer erro ocorrido durante o processamento
            _logger.LogError(ex, "Erro ao processar notificação FIWARE.");

            // O Orion espera apenas um HTTP 200, mesmo em caso de erro
            return Ok();
        }
    }

    // Processa notificações provenientes dos skids RFID
    private async Task ProcessSkidEvent(JsonElement entity)
    {
        // Obtém o suporte e a workstation da notificação
        if (!TryGetPropertyValue(entity, "supportId", out int supportId) ||
            !TryGetPropertyValue(entity, "currentWorkstation", out int workstationId))
        {
            _logger.LogWarning("Entidade sem supportId ou currentWorkstation. A ignorar.");
            return;
        }

        // Procura o suporte e a workstation na base de dados
        var support     = await _supportRepo.GetById(supportId);
        var workstation = await _workstationRepo.GetById(workstationId);

        if (support == null)
        {
            _logger.LogWarning("Support {SupportId} não encontrado.", supportId);
            return;
        }

        if (workstation == null)
        {
            _logger.LogWarning("Workstation {WorkstationId} não encontrada.", workstationId);
            return;
        }

        // Regista a deteção do skid
        _logger.LogInformation(
            "Skid {Tag} detectado na workstation {WsId} ({Phase})",
            support.RfidTag,
            workstationId,
            workstation.ManufacturingPhase?.Name ?? "?");

        // Guarda o histórico de localização do suporte
        await _localizationService.Create(
            new CreateLocalizationHistoryDTO(supportId, workstationId));

        // Verifica se existe um produto atualmente associado ao skid
        var supportedProduct = await _supportedProductRepo.GetCurrentBySupport(supportId);

        if (supportedProduct?.ProductId == null)
        {
            _logger.LogInformation(
                "Skid {Tag} sem produto associado. Só LocalizationHistory criado.",
                support.RfidTag);

            return;
        }

        // Se a workstation não estiver associada a uma fase de fabrico,
        // apenas fica registada a localização
        if (workstation.ManufacturingPhaseId == null)
        {
            _logger.LogInformation(
                "Workstation {WsId} sem fase associada. Só LocalizationHistory criado.",
                workstationId);

            return;
        }

        // Cria automaticamente uma nova ProductPhase
        var newPhase = await _productPhaseService.Create(
            new CreateProductPhaseDTO(
                ProductId: supportedProduct.ProductId.Value,
                ManufacturingPhaseId: workstation.ManufacturingPhaseId.Value,
                WorkstationId: workstationId,
                Notes: $"Detetado automaticamente via RFID - Tag: {support.RfidTag}"
            ));

        _logger.LogInformation(
            "ProductPhase criado para produto {ProductId} na fase {Phase}.",
            supportedProduct.ProductId,
            workstation.ManufacturingPhase?.Name);

        // Verifica se a sequência de produção está correta
        await CheckSequenceAndAlert(
            supportedProduct.ProductId.Value,
            newPhase.Id,
            workstation.ManufacturingPhaseId.Value);
    }

    // Recebe a notificação Orion quando a entidade Badge (crachá simulado) muda de
    // 'currentWorkstation'. Ao contrário do check-in manual (que usa o JWT do
    // utilizador logado), aqui o appUserId vem de um atributo estático do device
    // FIWARE — simula um crachá físico lido num leitor, independente de sessão.
    private async Task ProcessBadgeEvent(JsonElement entity)
    {
        // Obtém o utilizador e a workstation enviados pelo Orion
        if (!TryGetPropertyValue(entity, "appUserId", out int appUserId) ||
            !TryGetPropertyValue(entity, "currentWorkstation", out int workstationId))
        {
            _logger.LogWarning("Entidade Badge sem appUserId ou currentWorkstation. A ignorar.");
            return;
        }

        // Processa automaticamente o check-in/check-out do operador
        var (success, error, action) =
            await _presenceService.ProcessBadgeScan(appUserId, workstationId);

        if (success)
        {
            _logger.LogInformation(
                "Badge: utilizador {UserId} — {Action} na workstation {WsId}.",
                appUserId,
                action == "checkin" ? "check-in" : "check-out",
                workstationId);
        }
        else
        {
            _logger.LogWarning(
                "Badge: falha ao processar utilizador {UserId} na workstation {WsId}: {Error}",
                appUserId,
                workstationId,
                error);
        }
    }

    // Verifica se a transição de fase respeita a sequência esperada do modelo.
    // Considera o conjunto de fases já visitadas (não só a última), para distinguir
    // corretamente saltos (buracos novos) de correções (preencher buracos antigos).
    private async Task CheckSequenceAndAlert(int productId, int newPhaseId, int newManufacturingPhaseId)
    {
        ...
    }

    // Cria automaticamente um alerta quando é detetado um salto na sequência
    private async Task CreateSequenceAlert(...)
    {
        ...
    }

    // Resolve automaticamente alertas de sequência quando o problema é corrigido
    private async Task ResolvePendingSequenceAlerts(int productId)
    {
        ...
    }

    // Método auxiliar que extrai propriedades recebidas nas notificações NGSI-LD
    // convertendo-as para o tipo pretendido (int ou string)
    private static bool TryGetPropertyValue<T>(JsonElement entity, string propertyName, out T value)
    {
        ...
    }
}