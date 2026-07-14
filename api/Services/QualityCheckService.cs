using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Implementa o contrato definido em IQualityCheckService
public class QualityCheckService : IQualityCheckService
{
    // Repository responsável por consultar e guardar controlos de qualidade
    private readonly IQualityCheckRepository _repo;

    // Repository usado para consultar as regras de cada fase de fabrico
    private readonly IManufacturingPhaseRepository _phaseRepo;

    // O ASP.NET injeta automaticamente os dois repositories
    public QualityCheckService(
        IQualityCheckRepository repo,
        IManufacturingPhaseRepository phaseRepo)
    {
        _repo = repo;
        _phaseRepo = phaseRepo;
    }

    // Devolve todos os controlos de qualidade de um produto
    public async Task<IEnumerable<QualityCheckDTO>> GetByProduct(int productId)
    {
        // Vai buscar os controlos de qualidade à base de dados
        var items = await _repo.GetByProduct(productId);

        // Converte cada QualityCheckModel para QualityCheckDTO
        return items.Select(qc => new QualityCheckDTO(
            qc.Id,
            qc.ProductId,
            qc.ManufacturingPhaseId,
            qc.Notes,
            qc.Status,
            qc.Severity
        ));
    }

    // Devolve um controlo de qualidade através do seu ID
    public async Task<QualityCheckDTO?> GetById(int id)
    {
        // Procura o controlo de qualidade na base de dados
        var item = await _repo.GetById(id);

        // Se não existir, devolve null
        if (item == null) return null;

        // Converte o modelo encontrado para DTO
        return new QualityCheckDTO(
            item.Id,
            item.ProductId,
            item.ManufacturingPhaseId,
            item.Notes,
            item.Status,
            item.Severity
        );
    }

    // Cria um novo controlo de qualidade
    // O estado final é decidido automaticamente pelo sistema
    public async Task<QualityCheckDTO> Create(CreateQualityCheckDTO dto)
    {
        // 1. Obter as regras da fase atual
        // Cada fase define qual a severidade máxima aceitável
        // e a partir de que severidade é necessário retrabalho
        var phase = await _phaseRepo.GetById(dto.ManufacturingPhaseId);

        // Se a fase indicada não existir, interrompe a operação
        if (phase == null)
            throw new KeyNotFoundException(
                "Fase de fabrico não encontrada."
            );


        // 2. Lógica de decisão automática usando os pesos das constantes
        // Converte a severidade observada num valor numérico
        int weightObserved = Severity.GetWeight(dto.Severity);

        // Converte a severidade máxima aceitável da fase num valor numérico
        int weightMax = Severity.GetWeight(
            phase.MaxAcceptableSeverity
        );

        // Converte o limite de retrabalho da fase num valor numérico
        int weightRework = Severity.GetWeight(
            phase.ReworkSeverity
        );

        // Variável onde será guardado o resultado final
        string finalStatus;

        // Se a severidade observada estiver dentro do limite aceitável,
        // o controlo de qualidade fica aprovado
        if (weightObserved <= weightMax)
        {
            finalStatus = QualityStatus.Passed;
        }
        // Se ultrapassar o limite aceitável, mas ainda estiver dentro
        // do limite de retrabalho, o produto necessita de correção
        else if (weightObserved <= weightRework)
        {
            finalStatus = QualityStatus.Rework;
        }
        // Se ultrapassar também o limite de retrabalho,
        // o produto é considerado rejeitado/sucata
        else
        {
            finalStatus = QualityStatus.Scrapped;
        }


        // 3. Mapear para o modelo e gravar
        // Cria a entidade que será guardada na base de dados
        var entity = new QualityCheckModel
        {
            ProductId = dto.ProductId,
            ManufacturingPhaseId = dto.ManufacturingPhaseId,
            Severity = dto.Severity,
            Notes = dto.Notes,

            // O utilizador não escolhe o estado:
            // ele é calculado automaticamente pelo sistema
            Status = finalStatus
        };

        // Guarda o controlo de qualidade na base de dados
        var created = await _repo.Create(entity);

        // Converte a entidade criada para DTO e devolve-a
        return new QualityCheckDTO(
            created.Id,
            created.ProductId,
            created.ManufacturingPhaseId,
            created.Notes,
            created.Status,
            created.Severity
        );
    }
}