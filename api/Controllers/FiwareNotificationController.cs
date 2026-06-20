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
    private readonly IProductPhaseRepository _productPhaseRepo;
    private readonly IPhaseSequenceRepository _phaseSequenceRepo;
    private readonly IProductRepository _productRepo;
    private readonly IAlertRepository _alertRepo;
    private readonly IAlertService _alertService;
    private readonly ILogger<FiwareNotificationController> _logger;

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
        _logger               = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Notify([FromBody] JsonElement body)
    {
        try
        {
            _logger.LogInformation("Notificação FIWARE recebida: {Body}", body.ToString());

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
            return Ok();
        }
    }

    private async Task ProcessSkidEvent(JsonElement entity)
    {
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

        await _localizationService.Create(new CreateLocalizationHistoryDTO(supportId, workstationId));

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

        var newPhase = await _productPhaseService.Create(new CreateProductPhaseDTO(
            ProductId:            supportedProduct.ProductId.Value,
            ManufacturingPhaseId: workstation.ManufacturingPhaseId.Value,
            WorkstationId:        workstationId,
            Notes:                $"Detetado automaticamente via RFID - Tag: {support.RfidTag}"
        ));

        _logger.LogInformation(
            "ProductPhase criado para produto {ProductId} na fase {Phase}.",
            supportedProduct.ProductId, workstation.ManufacturingPhase?.Name);

        await CheckSequenceAndAlert(
            supportedProduct.ProductId.Value,
            newPhase.Id,
            workstation.ManufacturingPhaseId.Value);
    }

    // Verifica se a transição de fase respeita a sequência esperada do modelo.
    // Considera o conjunto de fases já visitadas (não só a última), para distinguir
    // corretamente saltos (buracos novos) de correções (preencher buracos antigos).
    private async Task CheckSequenceAndAlert(int productId, int newPhaseId, int newManufacturingPhaseId)
    {
        try
        {
            var product = await _productRepo.GetById(productId);
            if (product == null) return;

            var allPhases = await _productPhaseRepo.GetByProduct(productId);
            var closedPhases = allPhases
                .Where(pp => pp.DatetimeEnd != null && pp.Id != newPhaseId)
                .OrderBy(pp => pp.DatetimeIni)
                .ToList();

            var expectedSequence = await _phaseSequenceRepo.GetByModel(product.ModelId);
            var orderedSequence = expectedSequence.OrderBy(ps => ps.Order).ToList();

            var newPhaseOrder = orderedSequence
                .FirstOrDefault(ps => ps.ManufacturingPhaseId == newManufacturingPhaseId)?.Order;

            if (newPhaseOrder == null) return; // fase fora do modelo, não há o que comparar

            if (!closedPhases.Any())
            {
                // primeira fase do produto — só é válida se for a fase order=1
                if (newPhaseOrder.Value != 1)
                {
                    await CreateSequenceAlert(productId, newPhaseId, newManufacturingPhaseId, 0, newPhaseOrder.Value, orderedSequence);
                }
                return;
            }

            // Conjunto de orders já visitados (histórico completo, não só a última fase)
            var visitedOrders = closedPhases
                .Select(p => orderedSequence.FirstOrDefault(ps => ps.ManufacturingPhaseId == p.ManufacturingPhaseId)?.Order)
                .Where(o => o != null)
                .Select(o => o!.Value)
                .ToHashSet();

            var maxVisited = visitedOrders.Count > 0 ? visitedOrders.Max() : 0;

            if (visitedOrders.Contains(newPhaseOrder.Value))
            {
                // Repetição da mesma fase — sem alerta de sequência (caso já tratado visualmente como "repeated")
                return;
            }

            if (newPhaseOrder.Value == maxVisited + 1)
            {
                // Avanço correto para a fronteira imediatamente seguinte
                await ResolvePendingSequenceAlerts(productId);
                return;
            }

            if (newPhaseOrder.Value < maxVisited)
            {
                // Está a preencher um buraco deixado atrás — correção válida de um salto anterior
                await ResolvePendingSequenceAlerts(productId);
                return;
            }

            // Saltou para a frente, criando um buraco novo
            await CreateSequenceAlert(productId, newPhaseId, newManufacturingPhaseId, maxVisited, newPhaseOrder.Value, orderedSequence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar sequência para produto {ProductId}", productId);
        }
    }

    private async Task CreateSequenceAlert(
        int productId, int newPhaseId, int newManufacturingPhaseId,
        int fromOrder, int toOrder, List<Models.PhaseSequenceModel> orderedSequence)
    {
        var alreadyExists = await _alertRepo.ExistsOpenForPhaseAsync(newPhaseId, "wrong_sequence");
        if (alreadyExists) return;

        var product = await _productRepo.GetById(productId);
        var newManufacturingPhase = orderedSequence
            .FirstOrDefault(ps => ps.ManufacturingPhaseId == newManufacturingPhaseId)?.ManufacturingPhase;

        await _alertService.CreateAsync(
            type: "wrong_sequence",
            productId: productId,
            productPhaseId: newPhaseId,
            productSerial: product?.SerialNumber ?? productId.ToString(),
            phaseName: newManufacturingPhase?.Name ?? "?",
            orderFrom: fromOrder,
            orderTo: toOrder
        );

        _logger.LogWarning(
            "Alerta de sequência gerado para produto {ProductId}: order {From} → {To}",
            productId, fromOrder, toOrder);
    }

    private async Task ResolvePendingSequenceAlerts(int productId)
    {
        var pendingAlerts = await _alertRepo.GetPendingByProductAndTypeAsync(productId, "wrong_sequence");
        foreach (var alert in pendingAlerts)
        {
            await _alertService.ResolveAsync(alert.Id);
        }
    }

    private static bool TryGetPropertyValue<T>(JsonElement entity, string propertyName, out T value)
    {
        value = default!;
        if (!entity.TryGetProperty(propertyName, out var prop)) return false;

        JsonElement valueElement;
        if (prop.ValueKind == JsonValueKind.Object && prop.TryGetProperty("value", out valueElement))
        {
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