using ApiTexPact.DTO;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiTexPact.Controllers;

// Recebe notificações do Orion Context Broker quando um skid muda de workstation.
// O Orion chama este endpoint automaticamente via subscrição NGSI-LD.
[ApiController]
[Route("api/[controller]")]
public class FiwareNotificationController : ControllerBase
{
    private readonly ISupportRepository _supportRepo;
    private readonly IWorkstationRepository _workstationRepo;
    private readonly ILocalizationHistoryService _localizationService;
    private readonly IProductPhaseService _productPhaseService;
    private readonly ISupportedProductRepository _supportedProductRepo;
    private readonly ILogger<FiwareNotificationController> _logger;

    public FiwareNotificationController(
        ISupportRepository supportRepo,
        IWorkstationRepository workstationRepo,
        ILocalizationHistoryService localizationService,
        IProductPhaseService productPhaseService,
        ISupportedProductRepository supportedProductRepo,
        ILogger<FiwareNotificationController> logger)
    {
        _supportRepo          = supportRepo;
        _workstationRepo      = workstationRepo;
        _localizationService  = localizationService;
        _productPhaseService  = productPhaseService;
        _supportedProductRepo = supportedProductRepo;
        _logger               = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Notify([FromBody] JsonElement body)
    {
        try
        {
            _logger.LogInformation("Notificação FIWARE recebida: {Body}", body.ToString());

            // O Orion envia um array de entidades modificadas em "data"
            if (!body.TryGetProperty("data", out var dataArray))
            {
                _logger.LogWarning("Notificação sem campo 'data'.");
                return Ok();
            }

            foreach (var entity in dataArray.EnumerateArray())
            {
                await ProcessSkidEvent(entity);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar notificação FIWARE.");
            return Ok(); // Devolver sempre 200 ao Orion para não repetir notificações
        }
    }

    private async Task ProcessSkidEvent(JsonElement entity)
    {
        // Extrair supportId e currentWorkstation da entidade NGSI-LD
        if (!TryGetPropertyValue(entity, "supportId", out int supportId) ||
            !TryGetPropertyValue(entity, "currentWorkstation", out int workstationId))
        {
            _logger.LogWarning("Entidade sem supportId ou currentWorkstation. A ignorar.");
            return;
        }

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

        _logger.LogInformation(
            "Skid {Tag} detectado na workstation {WsId} ({Phase})",
            support.RfidTag, workstationId, workstation.ManufacturingPhase?.Name ?? "?");

        // 1. Criar LocalizationHistory (regista onde o skid está fisicamente)
        await _localizationService.Create(new CreateLocalizationHistoryDTO(supportId, workstationId));

        // 2. Se o skid tem um produto associado, criar ProductPhase
        var supportedProduct = await _supportedProductRepo.GetCurrentBySupport(supportId);
        if (supportedProduct?.ProductId == null)
        {
            _logger.LogInformation("Skid {Tag} sem produto associado. Só LocalizationHistory criado.", support.RfidTag);
            return;
        }

        if (workstation.ManufacturingPhaseId == null)
        {
            _logger.LogInformation("Workstation {WsId} sem fase associada. Só LocalizationHistory criado.", workstationId);
            return;
        }

        // Criar ProductPhase (regista que o produto entrou nesta fase)
        await _productPhaseService.Create(new CreateProductPhaseDTO(
            ProductId:            supportedProduct.ProductId.Value,
            ManufacturingPhaseId: workstation.ManufacturingPhaseId.Value,
            WorkstationId:        workstationId,
            Notes:                $"Detetado automaticamente via RFID - Tag: {support.RfidTag}"
        ));

        _logger.LogInformation(
            "ProductPhase criado para produto {ProductId} na fase {Phase}.",
            supportedProduct.ProductId, workstation.ManufacturingPhase?.Name);
    }

    // Extrai o valor de um atributo NGSI-LD.
    // O Orion envia { "atributo": { "type": "Property", "value": X } }
    private static bool TryGetPropertyValue<T>(JsonElement entity, string propertyName, out T value)
    {
        value = default!;
        if (!entity.TryGetProperty(propertyName, out var prop)) return false;

        // Tentar obter o "value" dentro do objeto NGSI-LD
        JsonElement valueElement;
        if (prop.ValueKind == JsonValueKind.Object && prop.TryGetProperty("value", out valueElement))
        {
            // é um atributo NGSI-LD { type, value }
        }
        else
        {
            valueElement = prop;
        }

        try
        {
            if (typeof(T) == typeof(int))
            {
                value = (T)(object)valueElement.GetInt32();
                return true;
            }
            if (typeof(T) == typeof(string))
            {
                value = (T)(object)(valueElement.GetString() ?? "");
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}