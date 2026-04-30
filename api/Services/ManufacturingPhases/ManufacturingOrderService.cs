using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingOrder;
using ApiTexPact.Services.Interface.ManufacturingOrder;

namespace ApiTexPact.Services;

public class ManufacturingOrderService : IManufacturingOrderService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IManufacturingOrderRepository _repository;

    public ManufacturingOrderService(IManufacturingOrderRepository repository, IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IEnumerable<ManufacturingOrderDTO>> GetAllOrders()
    {
        var orders = await _repository.GetAll();
        return orders.Select(ToDTO);
    }

    public async Task<ManufacturingOrderDTO> GetOrderById(int id)
    {
        var order = await _repository.GetById(id);
        return ToDTO(order);
    }

    public async Task<ManufacturingOrderDTO> CreateOrder(CreateManufacturingOrderDTO orderDto)
    {
        if (orderDto == null)
            throw new ArgumentNullException(nameof(orderDto), "ManufacturingOrderDTO cannot be null.");

        orderDto.ManufacturingOrderId = $"temp-order-{DateTime.UtcNow.Ticks}";

        var order = new ManufacturingOrderModel
        {
            OrderNumber = orderDto.OrderNumber,
            SheduleInit = orderDto.SheduleInit,
            Observations = orderDto.Observations,
            ManufacturingOrderId = orderDto.ManufacturingOrderId,
            ClientId = orderDto.ClientId,
            ManufacturingProcessId = orderDto.ManufacturingProcessId,
            ProductLotId = orderDto.ProductLotId
        };

        var created = await _repository.Create(order);

        created.ManufacturingOrderId = $"urn:ngsi-ld:ManufacturingOrder:{created.Id}";
        await _repository.Update(created);

        try
        {
            await CreateOnFiwareAsync(created);
            return ToDTO(created);
        }
        catch (Exception ex)
        {
            await _repository.Delete(created.Id);
            throw new Exception($"Error adding ManufacturingOrder to FIWARE: {ex.Message}");
        }
    }

    public async Task<ManufacturingOrderDTO> UpdateOrder(int id, UpdateManufacturingOrderDTO orderDto)
    {
        var existingOrder = await _repository.GetById(id);

        existingOrder.OrderNumber = orderDto.OrderNumber;
        existingOrder.SheduleInit = orderDto.SheduleInit;
        existingOrder.Observations = orderDto.Observations;
        existingOrder.ClientId = orderDto.ClientId;
        existingOrder.ManufacturingProcessId = orderDto.ManufacturingProcessId;
        existingOrder.ProductLotId = orderDto.ProductLotId;

        await _repository.Update(existingOrder);
        await UpdateOnFiwareAsync(existingOrder);

        return ToDTO(existingOrder);
    }

    public async Task DeleteOrder(int id)
    {
        await _repository.Delete(id);
    }

    public async Task<GraphDto> BuildGraphAsync(int manufacturingOrderId)
{
    var order = await _repository.GetByIdWithDetailsForGraphAsync(manufacturingOrderId);
    if (order == null)
        return new GraphDto();

    var graph = new GraphDto();
    var added = new HashSet<string>();
    var edgeSeq = 0;
    
    void AddNode(string id, string label, string type, int group, object? extra = null)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(label)) return;
        if (added.Add(id))
            graph.Nodes.Add(new GraphNode
            {
                Id = id,
                Label = label,
                Type = type,
                Group = group,
                AdditionalData = extra
            });
    }

    void AddEdge(string src, string tgt, string label)
    {
        if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(tgt)) return;
        if (!added.Contains(src) || !added.Contains(tgt)) return;

        graph.Edges.Add(new GraphEdge
        {
            Source = src,
            Target = tgt,
            Label = label,
            Sequence = ++edgeSeq
        });
    }
    
    var orderKey = $"order-{order.Id}";
    AddNode(orderKey, $"Encomenda #{order.OrderNumber}", "order", 1);

    if (order.Client != null)
    {
        var clientKey = $"client-{order.ClientId}";
        AddNode(clientKey, order.Client.Name, "client", 2);
        AddEdge(orderKey, clientKey, "cliente");
    }

    if (order.ProductLot != null)
    {
        var plKey = $"productLot-{order.ProductLotId}";
        AddNode(plKey, order.ProductLot.LotNumber, "productLot", 3);
        AddEdge(orderKey, plKey, "lote");
    }
    
    if (order.ManufacturingProcess != null)
    {
        var procKey = $"process-{order.ManufacturingProcessId}";
        AddNode(procKey, order.ManufacturingProcess.ProcessName, "process", 4);
        AddEdge(orderKey, procKey, "processo");

        var processPhases = order.ManufacturingProcess.ManufacturingProcessPhases
            ?.Where(p => p.ManufacturingPhase?.PlantFloorSection != null)
            .OrderBy(p => p.NumberStepOrder) ?? Enumerable.Empty<ManufacturingProcessPhaseModel>();

        foreach (var mpp in processPhases)
        {
            var phase = mpp.ManufacturingPhase!;
            var phaseKey = $"processPhase-{phase.Id}";
            var phaseName = phase.PhaseInfo ?? "Fase sem nome";

            AddNode(phaseKey, phaseName, "processPhase", 5);
            AddEdge(procKey, phaseKey, $"etapa {mpp.NumberStepOrder}");

            var section = phase.PlantFloorSection!;
            var sectKey = $"section-{section.SectionId}";
            AddNode(sectKey, section.name ?? "Secção sem nome", "section", 6);
            AddEdge(phaseKey, sectKey, "secção");
        }
    }
    
    var orderHistory = order.ManufacturingOrderHistory
        ?.Where(h => h.PlantFloorSectionId > 0)
        .OrderBy(h => h.DateTime) ?? Enumerable.Empty<ManufacturingOrderHistoryModel>();

    foreach (var h in orderHistory)
    {
        var sectKey = $"section-{h.PlantFloorSectionId}";
        AddNode(sectKey, $"Secção #{h.PlantFloorSectionId}", "section", 6);

        var histKey = $"history-{order.Id}-{h.PlantFloorSectionId}";
        AddNode(histKey, $"{h.StatusName} em {h.DateTime:yyyy-MM-dd}", "history", 7);

        AddEdge(orderKey, histKey, "histórico");
        AddEdge(sectKey, histKey, "histórico");
    }
    
    var orderPhases = order.ManufacturingOrderPhases
        ?.Where(p => p.ManufacturingPhase?.PlantFloorSection != null)
        .OrderBy(p => p.DateTimeInit) ?? Enumerable.Empty<ManufacturingOrderPhaseModel>();

    foreach (var mop in orderPhases)
    {
        var instKey = $"orderPhaseInstance-{mop.Id}";
        AddNode(instKey, $"{mop.CustomizationParams} ({mop.Quantity})", "orderPhaseInstance", 8);

        var section = mop.ManufacturingPhase!.PlantFloorSection!;
        var sectKey = $"section-{section.SectionId}";
        AddNode(sectKey, section.name ?? "Secção sem nome", "section", 6);

        AddEdge(sectKey, instKey, "fase");
    }
    
    var allItems = order.ItemsOfRawMaterial
        ?.Where(i => i.LotOfRawMaterial != null)
        .ToList() ?? new List<ItemOfRawMaterialModel>();

    Console.WriteLine($"DEBUG BuildGraph - Total items encontrados: {allItems.Count}");
    
    var itemsByContainer = allItems
        .Where(i => i.ItemInContainer?.Container != null)
        .GroupBy(i => i.ItemInContainer!.Container!)
        .ToDictionary(g => g.Key, g => g.ToList());

    Console.WriteLine($"DEBUG - Containers encontrados: {itemsByContainer.Count}");

    foreach (var containerGroup in itemsByContainer)
    {
        var container = containerGroup.Key;
        var containerItems = containerGroup.Value;

        Console.WriteLine($"DEBUG - Processando Container {container.ContainerId}:");
        Console.WriteLine($"  - Items no container: {containerItems.Count}");

        var contKey = $"container-{container.ContainerId}";
        AddNode(contKey, container.ContainerCode ?? "Container sem código", "container", 10);
        
        foreach (var item in containerItems)
        {
            var lot = item.LotOfRawMaterial!;
            var lotKey = $"rawLot-{lot.LotId}";

            var lotExtra = new
            {
                Lot = new { lot.LotId, lot.LotCode },
                Raw = lot.RawMaterials == null ? null : new
                {
                    lot.RawMaterials.RawId,
                    lot.RawMaterials.Name,
                    lot.RawMaterials.Info,
                },
                Summary = lot.RawMaterials == null
                    ? $"Lote {lot.LotCode}"
                    : $"Lote de {lot.RawMaterials.Name} ({lot.RawMaterials.Info})"
            };

            AddNode(lotKey, lot.LotCode ?? "Lote sem código", "rawLot", 9, lotExtra);
            AddEdge(orderKey, lotKey, "usa");
            AddEdge(lotKey, contKey, "conteúdo");
        }
        
        var containerLocalizations = container.LocalizationHistories
            ?.Where(l => l.SectionId > 0)
            .OrderBy(l => l.Datetime) ?? Enumerable.Empty<ContainerLocalizationModel>();

        foreach (var loc in containerLocalizations)
        {
            Console.WriteLine($"DEBUG - Processando localização {loc.Id} (Secção {loc.SectionId}):");
            
            var itemsInThisLocation = containerItems
                .Where(item => item.ItemLocalizations != null &&
                              item.ItemLocalizations.Any(il => il.ContainerLocalizationId == loc.Id))
                .Select(item => {
                    var localization = item.ItemLocalizations!
                        .FirstOrDefault(il => il.ContainerLocalizationId == loc.Id);
                    
                    return new
                    {
                        item.ItemRawId,
                        item.Quantity,
                        item.Unit,
                        item.LotOfRawMaterialId,
                        LotCode = item.LotOfRawMaterial!.LotCode,
                        RawMaterialName = item.LotOfRawMaterial.RawMaterials?.Name ?? "Desconhecido",
                        Date = localization?.DateTime ?? DateTime.MinValue
                    };
                })
                .ToList();

            Console.WriteLine($"  - Items encontrados nesta localização: {itemsInThisLocation.Count}");

            foreach (var itemLoc in itemsInThisLocation)
            {
                Console.WriteLine($"    - {itemLoc.RawMaterialName} ({itemLoc.Quantity} {itemLoc.Unit})");
            }

            var locExtra = new
            {
                Summary = itemsInThisLocation.Count == 0
                    ? "Container estava vazio nesta localização"
                    : $"Container tinha {itemsInThisLocation.Count} item(s): {string.Join(", ", itemsInThisLocation.Select(i => $"{i.RawMaterialName} ({i.Quantity} {i.Unit})"))}"
            };

            var locKey = $"location-{loc.Id}";
            AddNode(locKey, $"{loc.Datetime:yyyy-MM-dd HH:mm}", "location", 11, locExtra);
            AddEdge(contKey, locKey, "localização");

            var sectKey = $"section-{loc.SectionId}";
            AddNode(sectKey, $"Secção #{loc.SectionId}", "section", 6);
            AddEdge(locKey, sectKey, "secção");
        }
    }
    
    var itemsWithoutContainer = allItems.Where(i => i.ItemInContainer?.Container == null);

    foreach (var item in itemsWithoutContainer)
    {
        var lot = item.LotOfRawMaterial!;
        var lotKey = $"rawLot-{lot.LotId}";

        var lotExtra = new
        {
            Lot = new { lot.LotId, lot.LotCode },
            Raw = lot.RawMaterials == null ? null : new
            {
                lot.RawMaterials.RawId,
                lot.RawMaterials.Name,
                lot.RawMaterials.Info,
            },
            Summary = lot.RawMaterials == null
                ? $"Lote {lot.LotCode}"
                : $"Lote de {lot.RawMaterials.Name} ({lot.RawMaterials.Info})"
        };

        AddNode(lotKey, lot.LotCode ?? "Lote sem código", "rawLot", 9, lotExtra);
        AddEdge(orderKey, lotKey, "usa");
        
        var itemLocalizations = item.ItemLocalizations
            ?.Where(l => l.ContainerLocalizationId > 0)
            .OrderBy(l => l.DateTime) ?? Enumerable.Empty<ItemLocalizationModel>();

        foreach (var il in itemLocalizations)
        {
            var itemExtra = new
            {
                ItemInfo = new
                {
                    ItemRawId = item.ItemRawId,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    LotOfRawMaterialId = item.LotOfRawMaterialId,
                    LotCode = lot.LotCode,
                    RawMaterialName = lot.RawMaterials?.Name ?? "Desconhecido"
                },
                LocationInfo = new
                {
                    ItemLocalizationId = il.ItemLocalizationId,
                    ContainerLocalizationId = il.ContainerLocalizationId,
                    DateTime = il.DateTime
                },
                Summary = $"{lot.RawMaterials?.Name ?? "Material"} ({item.Quantity} {item.Unit}) em {il.DateTime:yyyy-MM-dd HH:mm}"
            };

            var ilKey = $"itemLocation-{il.ItemLocalizationId}";
            AddNode(ilKey, $"{lot.RawMaterials?.Name ?? "Material"} - {il.DateTime:yyyy-MM-dd HH:mm}", "itemLocation", 12, itemExtra);
            AddEdge(lotKey, ilKey, "movimenta-se");

            var viaKey = $"location-{il.ContainerLocalizationId}";
            AddEdge(ilKey, viaKey, "via");
        }
    }

    Console.WriteLine($"DEBUG - Processamento de gráfico concluído. Total nós: {graph.Nodes.Count}, Total arestas: {graph.Edges.Count}");
    return graph;
}


    private static ManufacturingOrderDTO ToDTO(ManufacturingOrderModel order)
    {
        return new ManufacturingOrderDTO
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            SheduleInit = order.SheduleInit,
            Observations = order.Observations,
            ManufacturingOrderId = order.ManufacturingOrderId,
            ClientId = order.ClientId,
            ManufacturingProcessId = order.ManufacturingProcessId,
            ProductLotId = order.ProductLotId
        };
    }

    private async Task CreateOnFiwareAsync(ManufacturingOrderModel order)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities";

        var payload = new Dictionary<string, object>
        {
            ["@context"] = new[] { "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" },
            ["id"] = order.ManufacturingOrderId,
            ["type"] = "ManufacturingOrder",
            ["orderNumber"] = new { type = "Property", value = order.OrderNumber },
            ["sheduleInit"] = new { type = "Property", value = order.SheduleInit },
            ["observations"] = new { type = "Property", value = order.Observations },
            ["clientId"] = new { type = "Relationship", @object = $"urn:ngsi-ld:Client:{order.ClientId}" },
            ["manufacturingProcessId"] = new
                { type = "Relationship", @object = $"urn:ngsi-ld:ManufacturingProcess:{order.ManufacturingProcessId}" },
            ["productLotId"] = new { type = "Relationship", @object = $"urn:ngsi-ld:ProductLot:{order.ProductLotId}" }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/ld+json");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/ld+json"));

        var response = await client.PostAsync(url, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"FIWARE creation failed: {response.StatusCode}, {responseBody}");
        }
    }

    private async Task UpdateOnFiwareAsync(ManufacturingOrderModel order)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities/{order.ManufacturingOrderId}/attrs";

        var payload = new Dictionary<string, object>
        {
            ["@context"] = new[] { "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" },
            ["orderNumber"] = new { type = "Property", value = order.OrderNumber },
            ["sheduleInit"] = new { type = "Property", value = order.SheduleInit },
            ["observations"] = new { type = "Property", value = order.Observations },
            ["clientId"] = new { type = "Relationship", @object = $"urn:ngsi-ld:Client:{order.ClientId}" },
            ["manufacturingProcessId"] = new
                { type = "Relationship", @object = $"urn:ngsi-ld:ManufacturingProcess:{order.ManufacturingProcessId}" },
            ["productLotId"] = new { type = "Relationship", @object = $"urn:ngsi-ld:ProductLot:{order.ProductLotId}" }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/ld+json");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/ld+json"));

        var response = await client.PatchAsync(url, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"FIWARE update failed: {response.StatusCode}, {responseBody}");
        }
    }
}
