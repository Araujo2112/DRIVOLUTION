using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiTexPact.DTO;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Texpact;
using Empty = Texpact.Empty;

namespace ApiTexPact.Controllers
{
    public class ChatHistoryRequest
    {
        public List<ChatMessage> Messages { get; set; }
    }

    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public enum QueryIntent
    {
        Prediction,
        Client,
        Container,
        RawMaterial,
        Product,
        Section,
        Checkpoint,
        Order,
        Localization,
        ModelTrain,
        Unknown
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ClientService.ClientServiceClient _grpcClients;
        private readonly ContainerService.ContainerServiceClient _grpcContainers;
        private readonly RawMaterialService.RawMaterialServiceClient _grpcRawMaterials;
        private readonly ProductService.ProductServiceClient _grpcProducts;
        private readonly PlantFloorSectionService.PlantFloorSectionServiceClient _grpcSections;
        private readonly CheckpointService.CheckpointServiceClient _grpcCheckpoints;
        private readonly ContainerLocalizationService.ContainerLocalizationServiceClient _grpcLocalizations;
        private readonly ManufacturingOrderService.ManufacturingOrderServiceClient _grpcOrders;
        private readonly ItemLocalizationService.ItemLocalizationServiceClient _grpcItemLoc;
        private readonly MlpPredictionService.MlpPredictionServiceClient _grpcPrediction;
        private readonly HttpClient _ollama;

        public ChatController(
            ClientService.ClientServiceClient grpcClients,
            ContainerService.ContainerServiceClient grpcContainers,
            RawMaterialService.RawMaterialServiceClient grpcRawMaterials,
            ProductService.ProductServiceClient grpcProducts,
            PlantFloorSectionService.PlantFloorSectionServiceClient grpcSections,
            CheckpointService.CheckpointServiceClient grpcCheckpoints,
            ContainerLocalizationService.ContainerLocalizationServiceClient grpcLocalizations,
            ManufacturingOrderService.ManufacturingOrderServiceClient grpcOrders,          
            ItemLocalizationService.ItemLocalizationServiceClient grpcItemLoc,
            MlpPredictionService.MlpPredictionServiceClient grpcPrediction,
            IHttpClientFactory httpFactory)
        {
            _grpcClients = grpcClients;
            _grpcContainers = grpcContainers;
            _grpcRawMaterials = grpcRawMaterials;
            _grpcProducts = grpcProducts;
            _grpcSections = grpcSections;
            _grpcCheckpoints = grpcCheckpoints;
            _grpcLocalizations = grpcLocalizations;
            _grpcOrders = grpcOrders;    
            _grpcItemLoc = grpcItemLoc;
            _grpcPrediction = grpcPrediction;
            _ollama = httpFactory.CreateClient("ollama");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatHistoryRequest req)
        {
            if (req?.Messages == null || !req.Messages.Any())
                return BadRequest("O campo 'messages' é obrigatório e não pode estar vazio.");

            var userQ = req.Messages.Last().Content.Trim();
            var intent = DetectIntent(userQ);

            switch (intent)
            {
                case QueryIntent.Prediction:
                    return await HandlePredictionRequest(userQ);
                
                case QueryIntent.Localization:
                    return await HandleLocalizationQuery(userQ);
                
                case QueryIntent.Order:
                    return await HandleOrderQuery(userQ);
                
                case QueryIntent.Client:
                    return await HandleClientQuery(userQ);
                
                case QueryIntent.Container:
                    return await HandleContainerQuery(userQ);
                
                case QueryIntent.RawMaterial:
                    return await HandleRawMaterialQuery(userQ);
                
                case QueryIntent.Product:
                    return await HandleProductQuery(userQ);
                
                case QueryIntent.Section:
                    return await HandleSectionQuery(userQ);
                
                case QueryIntent.Checkpoint:
                    return await HandleCheckpointQuery(userQ);
                
                case QueryIntent.ModelTrain:
                    return await HandleModelTrain();
                
                default:
                    return Ok(new { response = "Não compreendi a pergunta. Pode perguntar sobre clientes, contentores, produtos, matérias-primas, secções, checkpoints, ordens de fabrico, localizações ou previsões de produção." });
            }
        }

        private QueryIntent DetectIntent(string query)
        {
            var queryLower = query.ToLower();

            if (Regex.IsMatch(query, @"\b(previs(ão|ao)|prever|quantas|produzir|devo\s+(fabricar|produzir)|produção|tempo.*demora)\b", RegexOptions.IgnoreCase))
                return QueryIntent.Prediction;

            if (query.Contains("passou", StringComparison.OrdinalIgnoreCase) || 
                Regex.IsMatch(query, @"\bpassagens?\b", RegexOptions.IgnoreCase) ||
                query.Contains("localização", StringComparison.OrdinalIgnoreCase) ||
                Regex.IsMatch(query, @"contentores?\s+(?:que\s+)?passaram", RegexOptions.IgnoreCase))
                return QueryIntent.Localization;
            
            if (Regex.IsMatch(query, @"\bordens?\b", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(query, @"\bmat[eé]rias?.*\bordem\s+\d+", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(query, @"\bfases?.*\bordem\s+\d+", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(query, @"\bhist[oó]ric.*\bordem\s+\d+", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(query, @"\blocaliz.*\bordem\s+\d+", RegexOptions.IgnoreCase))
                return QueryIntent.Order;
            
            if (query.Contains("cliente", StringComparison.OrdinalIgnoreCase) && 
                (query.Contains("NIF", StringComparison.OrdinalIgnoreCase) || 
                 Regex.IsMatch(query, @"\b(lista|listar|dados|informação|detalhes)\b.*\bcliente", RegexOptions.IgnoreCase)))
                return QueryIntent.Client;
            
            if (query.Contains("contentor", StringComparison.OrdinalIgnoreCase))
                return QueryIntent.Container;

            if (query.Contains("matéria", StringComparison.OrdinalIgnoreCase) && 
                query.Contains("prima", StringComparison.OrdinalIgnoreCase) &&
                !query.Contains("ordem", StringComparison.OrdinalIgnoreCase))
                return QueryIntent.RawMaterial;

            if (query.Contains("produto", StringComparison.OrdinalIgnoreCase))
                return QueryIntent.Product;

            if (query.Contains("secção", StringComparison.OrdinalIgnoreCase) || 
                query.Contains("seção", StringComparison.OrdinalIgnoreCase))
                return QueryIntent.Section;

            if (query.Contains("checkpoint", StringComparison.OrdinalIgnoreCase))
                return QueryIntent.Checkpoint;

            if (query.Contains("treinar", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("reiniciar", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("aprender", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("modelo", StringComparison.OrdinalIgnoreCase))
                return QueryIntent.ModelTrain;

            return QueryIntent.Unknown;
        }

        private async Task<IActionResult> HandlePredictionRequest(string userQ)
        {
            var clientName = ExtractName(userQ, "cliente");
            var productName = ExtractName(userQ, "produto");
            var sectionName = ExtractName(userQ, "sec[cç][aã]o");
            var phaseName = ExtractPhase(userQ);
            var lotQty = ExtractNumber(userQ, "lote|lot");
            var hour = ExtractHour(userQ) ?? 8;
            var weekDay = ExtractWeekDay(userQ) ?? 1;
            var quantity = ExtractNumber(userQ, "quantidade|unidades|produzir|fabricar") ?? lotQty ?? 0;

            try
            {
                var clientId = await GetEntityIdFromRest<ClientDTO>(
                    clientName,
                    "http://localhost:5181/api/Client",
                    c => c.Name,
                    c => c.Id);

                var productId = await GetEntityIdFromRest<ProductDTO>(
                    productName,
                    "http://localhost:5181/api/Product",
                    p => p.Name,
                    p => p.Id);

                var sectionId = await GetEntityIdFromRest<PlantFloorSectionDTO>(
                    sectionName,
                    "http://localhost:5181/api/PlantFloorSection",
                    s => s.Name,
                    s => s.SectionId);
                

                var phaseId = await GetEntityIdFromRest<ManufacturingPhaseDTO>(
                    phaseName,
                    "http://localhost:5181/api/ManufacturingPhase",
                    p => p.PhaseInfo ?? "",
                    p => p.Id);
                
                if (sectionId == null && phaseId != null)
                {
                    try
                    {
                        using var client = new HttpClient();
                        var json = await client.GetStringAsync($"http://localhost:5181/api/ManufacturingPhase/{phaseId}");
                        var phaseDetails = JsonSerializer.Deserialize<ManufacturingPhaseDTO>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        sectionId = phaseDetails?.PlantFloorSectionId;

                    }
                    catch
                    {
                        return Ok(new { response = "Não foi possível obter a secção associada à fase indicada." });
                    }
                }

                if (clientId == null)
                    return Ok(new { response = $"Cliente '{clientName}' não encontrado." });
                if (productId == null)
                    return Ok(new { response = $"Produto '{productName}' não encontrado." });
                if (sectionId == null)
                    return Ok(new { response = $"Secção '{sectionName}' não encontrada." });
                if (phaseId == null)
                    return Ok(new { response = $"Fase '{phaseName}' não encontrada." });
                if (lotQty == null)
                    return Ok(new { response = "Quantidade do lote não especificada." });
                
                var req = new PredictRequest
                {
                    Features = {
                        new PredictFeature {
                            Quantity = quantity,
                            PhaseId = phaseId.Value,
                            SectionId = sectionId.Value,
                            ProductId = productId.Value,
                            ClientId = clientId.Value,
                            LotQty = lotQty.Value,
                            Hour = hour,
                            WeekDay = weekDay
                        }
                    }
                };

                var result = await _grpcPrediction.PredictAsync(req);
                var predictionSeconds = result.Predictions.FirstOrDefault();
                var predictionHours = predictionSeconds / 3600.0;

                return Ok(new
                {
                    response = $"Tempo estimado de produção: {predictionSeconds:F0} segundos (~{predictionHours:F2} horas) para {quantity} unidades do produto '{productName}' para o cliente '{clientName}'."
                });
            }
            catch (RpcException ex)
            {
                return Ok(new { response = $"Erro ao prever: {ex.Status.Detail}" });
            }
            catch (Exception ex)
            {
                return Ok(new { response = $"Erro interno: {ex.Message}" });
            }
        }

        private async Task<IActionResult> HandleLocalizationQuery(string userQ)
        {
            if (Regex.IsMatch(userQ, @"\b(lista|listar)\b.*\bpassagens?\b", RegexOptions.IgnoreCase))
            {
                var items = (await _grpcLocalizations.ListContainerLocalizationsAsync(new Empty())).Items.Take(100);
                var simplified = items.Select(i => new {
                    i.Id,
                    i.ContainerId,
                    i.SectionId,
                    i.Datetime,
                    i.ContainerName,
                    i.SectionName
                }).ToList();
                return await FormatWithOllama("lista de passagens de contentores", JsonSerializer.Serialize(simplified));
            }
            
            var mTime = Regex.Match(userQ,
                @"passaram pela sec[cç][aã]o\s+(\d+)\s+à hora\s+([\d:-T]+)",
                RegexOptions.IgnoreCase);
            if (mTime.Success)
            {
                var secId = int.Parse(mTime.Groups[1].Value);
                var when = mTime.Groups[2].Value;
                var resp = await _grpcLocalizations.ListBySectionAtTimeAsync(
                    new SectionTimeRequest { SectionId = secId, Datetime = when });
                return await FormatWithOllama(
                    $"contentores na secção {secId} à hora {when}",
                    JsonSerializer.Serialize(resp.Items)
                );
            }
            
            var mSecPass = Regex.Match(userQ,
                @"(?:quais\s+os\s+|listar\s+)?contentores?\s+(?:que\s+)?passaram\s+pela\s+sec[cç][aã]o\s+(\d+)",
                RegexOptions.IgnoreCase);
            if (mSecPass.Success)
            {
                var secId = int.Parse(mSecPass.Groups[1].Value);
                try
                {
                    var items = (await _grpcLocalizations.ListBySectionAsync(
                        new ContainerLocalizationBySectionRequest { SectionId = secId })).Items;
                    if (!items.Any())
                        return Ok(new { response = $"Nenhum contentor passou pela secção {secId}." });

                    return await FormatWithOllama(
                        $"contentores que passaram pela secção {secId}",
                        JsonSerializer.Serialize(items)
                    );
                }
                catch (RpcException ex)
                {
                    return Ok(new { response = $"Erro ao obter contentores da secção {secId}: {ex.Status.Detail}" });
                }
            }
            
            var mContPass = Regex.Match(userQ,
                @"(?:em\s+que\s+sec[cç][õeons]|por\s+onde)\s+passou\s+o\s+contentor\s+(\d+)|contentor\s+(\d+)\s+(?:passou\s+por|em\s+que\s+sec[cç]|por\s+onde)",
                RegexOptions.IgnoreCase);
            if (mContPass.Success)
            {
                var contId = mContPass.Groups[1].Success
                    ? mContPass.Groups[1].Value
                    : mContPass.Groups[2].Value;
                try
                {
                    var containerId = int.Parse(contId);
                    var items = (await _grpcLocalizations.ListSectionsByContainerAsync(
                        new ContainerLocalizationByContainerRequest { ContainerId = containerId })).Items;
                    if (!items.Any())
                        return Ok(new { response = $"O contentor {containerId} não passou por nenhuma secção registada." });

                    return await FormatWithOllama(
                        $"secções por onde passou o contentor {containerId}",
                        JsonSerializer.Serialize(items)
                    );
                }
                catch (FormatException)
                {
                    return Ok(new { response = $"ID do contentor '{contId}' inválido. Deve ser um número." });
                }
                catch (RpcException ex)
                {
                    return Ok(new { response = $"Erro ao obter secções do contentor {contId}: {ex.Status.Detail}" });
                }
            }

            return Ok(new { response = "Pergunta sobre localização não reconhecida." });
        }

        private async Task<IActionResult> HandleClientQuery(string userQ)
        {
            var mNif = Regex.Match(userQ, @"\bNIF\b.*\bcliente\s+(.+?)[?\.]?$", RegexOptions.IgnoreCase);
            if (mNif.Success)
            {
                var list = (await _grpcClients.ListClientsAsync(new Empty())).Clients;
                var matched = FindBestMatch(list, c => c.Name, mNif.Groups[1].Value.Trim());
                if (matched == null)
                    return Ok(new { response = $"Cliente '{mNif.Groups[1].Value}' não encontrado." });
                return Ok(new { response = $"O NIF de '{matched.Name}' é {matched.FiscalNumber}." });
            }
            
            if (Regex.IsMatch(userQ, @"\b(lista|listar)\b.*\bclientes\b", RegexOptions.IgnoreCase))
            {
                var items = (await _grpcClients.ListClientsAsync(new Empty())).Clients.Take(50);
                var simplified = items.Select(c => new { c.Id, c.Name, c.FiscalNumber }).ToList();
                return await FormatWithOllama("lista de clientes", JsonSerializer.Serialize(simplified));
            }
            
            var mCli = Regex.Match(userQ, @"\bcliente\s+(.+?)[?\.]?$", RegexOptions.IgnoreCase);
            if (mCli.Success)
            {
                var list = (await _grpcClients.ListClientsAsync(new Empty())).Clients;
                var matched = FindBestMatch(list, c => c.Name, mCli.Groups[1].Value);
                if (matched == null)
                    return Ok(new { response = $"Cliente '{mCli.Groups[1].Value}' não encontrado." });

                var simplified = new { matched.Id, matched.Name, matched.FiscalNumber };
                return await FormatWithOllama($"detalhes do cliente {matched.Name}", JsonSerializer.Serialize(simplified));
            }

            return Ok(new { response = "Pergunta sobre cliente não reconhecida." });
        }

        private async Task<IActionResult> HandleContainerQuery(string userQ)
        {
            var mCont = Regex.Match(userQ, @"(?:informação|dados|detalhes).*?contentor\s+(\d+|[a-zA-Z]+)|contentor\s+(\d+|[a-zA-Z]+)(?:\s*[?\.]|\s*$)", RegexOptions.IgnoreCase);
            if (mCont.Success && 
                !userQ.Contains("passou", StringComparison.OrdinalIgnoreCase) && 
                !userQ.Contains("onde", StringComparison.OrdinalIgnoreCase))
            {
                var containerIdentifier = mCont.Groups[1].Success ? mCont.Groups[1].Value : mCont.Groups[2].Value;
                var list = (await _grpcContainers.ListContainersAsync(new Empty())).Containers;

                Texpact.Container matched = null;

                if (int.TryParse(containerIdentifier, out int containerId))
                {
                    matched = list.FirstOrDefault(c => c.ContainerId == containerId);
                }

                if (matched == null)
                {
                    matched = FindBestMatch(list, c => c.ContainerName.ToString(), containerIdentifier);
                }

                if (matched == null)
                    return Ok(new { response = $"Contentor '{containerIdentifier}' não encontrado." });

                var simplified = new { 
                    matched.ContainerId, 
                    ContainerName = matched.ContainerName?.ToString(), 
                    matched.ContainerVolume,
                    matched.Activate
                };
                return await FormatWithOllama($"detalhes do contentor {simplified.ContainerName}", JsonSerializer.Serialize(simplified));
            }
            
            if (Regex.IsMatch(userQ, @"\b(lista|listar)\b.*\bcontentores?\b", RegexOptions.IgnoreCase))
            {
                var items = (await _grpcContainers.ListContainersAsync(new Empty())).Containers.Take(50);
                var simplified = items.Select(c => new { 
                    c.ContainerId, 
                    ContainerName = c.ContainerName?.ToString(), 
                    c.ContainerVolume,
                    c.Activate
                }).ToList();
                return await FormatWithOllama("lista de contentores", JsonSerializer.Serialize(simplified));
            }

            return Ok(new { response = "Pergunta sobre contentor não reconhecida." });
        }

        private async Task<IActionResult> HandleRawMaterialQuery(string userQ)
        {
            if (Regex.IsMatch(userQ, @"\b(lista|listar)\b.*\bmat[eé]rias[- ]primas\b", RegexOptions.IgnoreCase))
            {
                var items = (await _grpcRawMaterials.ListRawMaterialsAsync(new Empty())).Rawmaterials.Take(50);
                var simplified = items.Select(r => new { r.RawId, Name = r.Name?.Name, Info = r.Info?.Name }).ToList();
                return await FormatWithOllama("lista de matérias-primas", JsonSerializer.Serialize(simplified));
            }
            
            var mRaw = Regex.Match(userQ, @"\bmat[eé]ria[- ]prima\s+(.+?)[?\.]?$", RegexOptions.IgnoreCase);
            if (mRaw.Success)
            {
                var list = (await _grpcRawMaterials.ListRawMaterialsAsync(new Empty())).Rawmaterials;
                var matched = FindBestMatch(list, r => r.Name?.Name ?? "", mRaw.Groups[1].Value);
                if (matched == null)
                    return Ok(new { response = $"Matéria-prima '{mRaw.Groups[1].Value}' não encontrada." });

                var simplified = new { matched.RawId, Name = matched.Name?.Name, Info = matched.Info?.Name };
                return await FormatWithOllama($"detalhes da matéria-prima {simplified.Name}", JsonSerializer.Serialize(simplified));
            }

            return Ok(new { response = "Pergunta sobre matéria-prima não reconhecida." });
        }

        private async Task<IActionResult> HandleProductQuery(string userQ)
        {
            if (Regex.IsMatch(userQ, @"\b(lista|listar)\b.*\bprodutos?\b", RegexOptions.IgnoreCase))
            {
                var items = (await _grpcProducts.ListProductsAsync(new Empty())).Products.Take(50);
                var simplified = items.Select(p => new { 
                    p.Id, 
                    Name = p.Name?.Name, 
                    Info = p.Info?.Name,
                    ProductId = p.ProductId?.Name
                }).ToList();
                return await FormatWithOllama("lista de produtos", JsonSerializer.Serialize(simplified));
            }
            
            var mProd = Regex.Match(userQ, @"\bproduto\s+(.+?)[?\.]?$", RegexOptions.IgnoreCase);
            if (mProd.Success)
            {
                var list = (await _grpcProducts.ListProductsAsync(new Empty())).Products;
                var matched = FindBestMatch(list, p => p.Name?.Name ?? "", mProd.Groups[1].Value);
                if (matched == null)
                    return Ok(new { response = $"Produto '{mProd.Groups[1].Value}' não encontrado." });

                var simplified = new { 
                    matched.Id, 
                    Name = matched.Name?.Name, 
                    Info = matched.Info?.Name,
                    ProductId = matched.ProductId?.Name
                };
                return await FormatWithOllama($"detalhes do produto {simplified.Name}", JsonSerializer.Serialize(simplified));
            }

            return Ok(new { response = "Pergunta sobre produto não reconhecida." });
        }

        private async Task<IActionResult> HandleSectionQuery(string userQ)
        {
            if (Regex.IsMatch(userQ, @"\b(lista|listar)\b.*\bsec[cç][aã]o(?:es)?\b", RegexOptions.IgnoreCase))
            {
                var items = (await _grpcSections.ListPlantFloorSectionsAsync(new Empty())).Sections.Take(50);
                var simplified = items.Select(s => new { s.SectionId, Name = s.Name?.Name }).ToList();
                return await FormatWithOllama("lista de secções", JsonSerializer.Serialize(simplified));
            }
            
            var mSec = Regex.Match(userQ, @"\bsec[cç][aã]o\s+(.+?)[?\.]?$", RegexOptions.IgnoreCase);
            if (mSec.Success)
            {
                var list = (await _grpcSections.ListPlantFloorSectionsAsync(new Empty())).Sections;
                var matched = FindBestMatch(list, s => s.Name?.Name ?? "", mSec.Groups[1].Value);
                if (matched == null)
                    return Ok(new { response = $"Secção '{mSec.Groups[1].Value}' não encontrada." });

                var simplified = new { matched.SectionId, Name = matched.Name?.Name };
                return await FormatWithOllama($"detalhes da secção {simplified.Name}", JsonSerializer.Serialize(simplified));
            }

            return Ok(new { response = "Pergunta sobre secção não reconhecida." });
        }

        private async Task<IActionResult> HandleCheckpointQuery(string userQ)
        {
         
            if (Regex.IsMatch(userQ, @"\b(lista|listar)\b.*\bcheckpoint(?:s)?\b", RegexOptions.IgnoreCase))
            {
                var items = (await _grpcCheckpoints.ListCheckpointsAsync(new Empty())).Checkpoints.Take(50);
                var simplified = items.Select(c => new { 
                    c.CheckpointId, 
                    c.Id,
                    Name = c.Name?.Name,
                    c.Status,
                    c.SectionId
                }).ToList();
                return await FormatWithOllama("lista de checkpoints", JsonSerializer.Serialize(simplified));
            }
            
            var mCp = Regex.Match(userQ, @"\bcheckpoint\s+(\d+)", RegexOptions.IgnoreCase);
            if (mCp.Success)
            {
                var list = (await _grpcCheckpoints.ListCheckpointsAsync(new Empty())).Checkpoints;
                var matchedId = int.Parse(mCp.Groups[1].Value);
                var matched = list.FirstOrDefault(c => c.CheckpointId == matchedId);

                if (matched == null)
                    return Ok(new { response = $"Checkpoint {matchedId} não encontrado." });

                var simplified = new { 
                    matched.CheckpointId, 
                    matched.Id,
                    Name = matched.Name?.Name,
                    matched.Status,
                    matched.SectionId
                };
                return await FormatWithOllama($"detalhes do checkpoint {matchedId}", JsonSerializer.Serialize(simplified));
            }

            return Ok(new { response = "Pergunta sobre checkpoint não reconhecida." });
        }

        private async Task<IActionResult> HandleOrderQuery(string userQ)
        {
            if (Regex.IsMatch(userQ, @"\b(lista|listar)\b.*\bordens?\b", RegexOptions.IgnoreCase))
            {
                var items = (await _grpcOrders.ListAsync(new Empty())).Items.Take(50);
                return await FormatWithOllama("lista de ordens de fabrico", JsonSerializer.Serialize(items));
            }
            
            var mDetalhar = Regex.Match(userQ, @"\b(?:detalhar|detalhes?|informação|dados)\s+(?:da\s+|das\s+)?ordens?\s+(?:de\s+fabrico\s+)?(\d+)", RegexOptions.IgnoreCase);
            if (mDetalhar.Success)
            {
                var idReq = new OrderIdRequest { OrderId = int.Parse(mDetalhar.Groups[1].Value) };
                var ord = await _grpcOrders.GetByIdAsync(idReq);
                return await FormatWithOllama($"detalhes da ordem {ord.Id}", JsonSerializer.Serialize(ord));
            }
            
            var mOrdPlural = Regex.Match(userQ, @"\bordens\s+de\s+fabrico\s+(\d+)", RegexOptions.IgnoreCase);
            if (mOrdPlural.Success)
            {
                var idReq = new OrderIdRequest { OrderId = int.Parse(mOrdPlural.Groups[1].Value) };
                var ord = await _grpcOrders.GetByIdAsync(idReq);
                return await FormatWithOllama($"detalhes da ordem {ord.Id}", JsonSerializer.Serialize(ord));
            }

            var mOrdFlex = Regex.Match(userQ, @"\bordens?\s+(\d+)", RegexOptions.IgnoreCase);
            if (mOrdFlex.Success)
            {
                var idReq = new OrderIdRequest { OrderId = int.Parse(mOrdFlex.Groups[1].Value) };
                var ord = await _grpcOrders.GetByIdAsync(idReq);
                return await FormatWithOllama($"detalhes da ordem {ord.Id}", JsonSerializer.Serialize(ord));
            }
            
            var mOrd = Regex.Match(userQ, @"\bordem(?:\s+de)?\s+fabrico\s+(\d+)", RegexOptions.IgnoreCase);
            if (mOrd.Success)
            {
                var idReq = new OrderIdRequest { OrderId = int.Parse(mOrd.Groups[1].Value) };
                var ord = await _grpcOrders.GetByIdAsync(idReq);
                return await FormatWithOllama($"detalhes da ordem {ord.Id}", JsonSerializer.Serialize(ord));
            }
            
            var mMatOrd = Regex.Match(userQ, @"\bmat[eé]rias?.*\bordem\s+(\d+)", RegexOptions.IgnoreCase);
            if (mMatOrd.Success)
            {
                var id = int.Parse(mMatOrd.Groups[1].Value);
                var reqM = new OrderIdRequest { OrderId = id };
                var raws = await _grpcOrders.ListRawMaterialsByOrderAsync(reqM);
                return await FormatWithOllama($"matérias-primas da ordem {id}", JsonSerializer.Serialize(raws.Items));
            }
            
            var mFaseOrd = Regex.Match(userQ, @"\bfases?.*\bordem\s+(\d+)", RegexOptions.IgnoreCase);
            if (mFaseOrd.Success)
            {
                var id = int.Parse(mFaseOrd.Groups[1].Value);
                var reqP = new OrderIdRequest { OrderId = id };
                var phs = await _grpcOrders.ListPhasesByOrderAsync(reqP);
                return await FormatWithOllama($"fases da ordem {id}", JsonSerializer.Serialize(phs.Items));
            }
            
            var mHistOrd = Regex.Match(userQ, @"\bhist[oó]ric.*\bordem\s+(\d+)", RegexOptions.IgnoreCase);
            if (mHistOrd.Success)
            {
                var id = int.Parse(mHistOrd.Groups[1].Value);
                var reqH = new OrderIdRequest { OrderId = id };
                var hist = await _grpcOrders.ListHistoryByOrderAsync(reqH);
                return await FormatWithOllama($"histórico da ordem {id}", JsonSerializer.Serialize(hist.Items));
            }
            
            var mLocOrd = Regex.Match(userQ, @"\blocaliz.*\bordem\s+(\d+)", RegexOptions.IgnoreCase);
            if (mLocOrd.Success)
            {
                var id = int.Parse(mLocOrd.Groups[1].Value);
                var reqL = new OrderIdRequest { OrderId = id };
                var locs = await _grpcItemLoc.ListByOrderItemsAsync(reqL);
                return await FormatWithOllama($"localizações de itens da ordem {id}", JsonSerializer.Serialize(locs.Items));
            }

            return Ok(new { response = "Pergunta sobre ordem de fabrico não reconhecida." });
        }
        private async Task<IActionResult> HandleModelTrain()
        {
            try
            {
                var resp = await _grpcPrediction.TrainAsync(new Empty());
                
                if (!string.IsNullOrWhiteSpace(resp.Message) && resp.Message.Contains("Sem dados novos", StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new { response = resp.Message });
                }

                var resumo = new
                {
                    Mensagem = resp.Message,
                    Versão = resp.Version,
                    TreinadoAté = resp.TrainedUntil
                };

                return await FormatWithOllama("resultado do treino do modelo", JsonSerializer.Serialize(resumo));
            }
            catch (RpcException ex)
            {
                return Ok(new { response = $"Erro ao treinar o modelo: {ex.Status.Detail}" });
            }
        }

        
        private string ExtractName(string input, string label)
        {
            var patterns = new[]
            {
               
                @$"{label}\s+(urn:ngsi-ld:[\w:]+)",
                
                @$"{label}\s+([A-ZÁ-Úa-zá-ú]+(?:\s+[A-ZÁ-Úa-zá-ú]+)*)\s+(?=na\s+fase|no\s+|num\s+|com\s+|às\s+|à\s+|em\s+|de\s+|para\s+)",
                
                @$"{label}\s+([A-ZÁ-Úa-zá-ú]+(?:\s+[A-ZÁ-Úa-zá-ú]+)*)\s*(?=\?|\.|$)",
                
                @$"{label}\s+([A-ZÁ-Úa-zá-ú]+(?:\s+[A-ZÁ-Úa-zá-ú]+)*)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var name = match.Groups[1].Value.Trim();
                    
                    if (!name.StartsWith("urn:", StringComparison.OrdinalIgnoreCase))
                    {
                        name = Regex.Replace(name, @"\s+(na|no|num|com|às|à|em|de|para).*", "", RegexOptions.IgnoreCase);
                    }

                    return name;
                }
            }

            return null;
        }


        private string ExtractPhase(string input)
        {
            var patterns = new[]
            {
                @"(?:na\s+fase\s+de\s+|fase\s+)([A-ZÁ-Úa-zá-ú]+(?:\s+[A-ZÁ-Úa-zá-ú]+)*)\s+(?=com\s+|na\s+|no\s+|às\s+|à\s+)",
                @"(?:na\s+fase\s+de\s+|fase\s+)([A-ZÁ-Úa-zá-ú]+(?:\s+[A-ZÁ-Úa-zá-ú]+)*)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            return null;
        }

        private int? ExtractNumber(string input, string label)
        {
         
            var match = Regex.Match(input, $@"(?:{label})\s+(?:de\s+)?(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
                return int.Parse(match.Groups[1].Value);
            
            var match2 = Regex.Match(input, $@"(?:{label})\s+(?:de\s+)?(\d+)\s+unidades?", RegexOptions.IgnoreCase);
            if (match2.Success)
                return int.Parse(match2.Groups[1].Value);

            return null;
        }


        private int? ExtractHour(string input)
        {
            var match = Regex.Match(input, @"às?\s+(\d{1,2})h?", RegexOptions.IgnoreCase);
            return match.Success ? int.Parse(match.Groups[1].Value) : null;
        }

        private int? ExtractWeekDay(string input)
        {
            var weekdays = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "segunda", 1 }, { "segunda-feira", 1 },
                { "terça", 2 }, { "terça-feira", 2 },
                { "quarta", 3 }, { "quarta-feira", 3 },
                { "quinta", 4 }, { "quinta-feira", 4 },
                { "sexta", 5 }, { "sexta-feira", 5 },
                { "sábado", 6 }, { "sabado", 6 },
                { "domingo", 7 }
            };

            foreach (var (day, value) in weekdays)
            {
                if (input.Contains(day, StringComparison.OrdinalIgnoreCase))
                    return value;
            }

            return null;
        }

        private async Task<int?> GetEntityIdFromRest<T>(
            string name,
            string endpoint,
            Func<T, string> getName,
            Func<T, int> getId)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            try
            {
                using var client = new HttpClient();
                var json = await client.GetStringAsync(endpoint);
                var list = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (list == null) return null;

                var match = list.FirstOrDefault(i => 
                    getName(i).Equals(name, StringComparison.OrdinalIgnoreCase));
                    
                return match != null ? getId(match) : null;
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private T FindBestMatch<T>(IEnumerable<T> items, Func<T, string> getName, string query)
        {
            var best = items.Select(x => (x, sim: LevenshteinSimilarity(getName(x) ?? "", query)))
                            .OrderByDescending(t => t.sim)
                            .FirstOrDefault();
            return best.sim >= 0.6 ? best.x : default;
        }

        private async Task<IActionResult> FormatWithOllama(string contexto, string rawJson)
        {
            string prompt;

            if (contexto.Contains("secções por onde passou", StringComparison.OrdinalIgnoreCase))
            {
                prompt = $@"system: És o assistente da TexP@ct especializado em rastreamento de contentores.
system: O utilizador perguntou por onde passou um contentor. Os dados fornecidos representam as secções da planta (PlantFloorSection) por onde esse contentor passou.
system: Devolve uma lista clara e ordenada de secções (nome e ID), indicando também a data e hora se disponíveis.
system: Mantém a resposta em Português de Portugal, clara e concisa.
dados_json: {rawJson}
assistant:";
            }
            else if (contexto.Contains("cliente", StringComparison.OrdinalIgnoreCase))
            {
                prompt = $@"system: És o assistente oficial da TexP@ct especializado em dados de clientes.
system: Formata a resposta como um cartão de visita profissional, incluindo apenas:
- Nome completo do cliente
- NIF (se disponível)
- ID interno
system: Mantém a resposta em Português de Portugal, formal mas concisa.
dados_json: {rawJson}
assistant:";
            }
            else if (contexto.Contains("contentor", StringComparison.OrdinalIgnoreCase))
            {
                prompt = $@"system: És o assistente oficial da TexP@ct especializado em contentores.
system: Formata a resposta destacando:
- ID do contentor
- Nome/designação
- Volume (se disponível), apresentado apenas como número seguido de 'un.' (ex: 500 un.)
- Estado (ativo/inativo)
system: Usa sempre Português de Portugal.
system: Sempre que apresentares volumes, usa o formato: número seguido de 'un.', sem escrever 'unidades' ou qualquer outra unidade.
dados_json: {rawJson}
assistant:";
            }
            else
            {
                prompt = $@"system: És o assistente oficial da TexP@ct.
system: Respondes sempre em Português de Portugal, de forma clara e concisa.
system: Recebes dados em JSON relativos a '{contexto}'.
system: Formata-os como um relatório humano organizado em bullet-points, destacando fields importantes (IDs, datas, nomes, etc.).
dados_json: {rawJson}
assistant:";
            }

            var body = new
            {
                model = "mistral",
                prompt,
                stream = false,
                temperature = 0.3
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await _ollama.PostAsync("generate", content);
            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var text = doc.RootElement.GetProperty("response").GetString()!.Trim();
            return Ok(new { response = text });
        }

        private static double LevenshteinSimilarity(string a, string b)
        {
            int dist = LevenshteinDistance(a.ToLower(), b.ToLower());
            int max = Math.Max(a.Length, b.Length);
            return max == 0 ? 1.0 : 1.0 - (double)dist / max;
        }

        private static int LevenshteinDistance(string s, string t)
        {
            var v0 = new int[t.Length + 1];
            var v1 = new int[t.Length + 1];
            for (int i = 0; i <= t.Length; i++) v0[i] = i;
            for (int i = 0; i < s.Length; i++)
            {
                v1[0] = i + 1;
                for (int j = 0; j < t.Length; j++)
                {
                    int cost = s[i] == t[j] ? 0 : 1;
                    v1[j + 1] = Math.Min(Math.Min(v1[j] + 1, v0[j + 1] + 1), v0[j] + cost);
                }
                Array.Copy(v1, v0, v0.Length);
            }
            return v0[t.Length];
        }
    }
}
