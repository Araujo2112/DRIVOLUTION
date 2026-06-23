using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class QualityCheckService : IQualityCheckService
{
    private readonly IQualityCheckRepository _repo;
    private readonly IManufacturingPhaseRepository _phaseRepo;

    public QualityCheckService(IQualityCheckRepository repo, IManufacturingPhaseRepository phaseRepo)
    {
        _repo = repo;
        _phaseRepo = phaseRepo;
    }

    public async Task<IEnumerable<QualityCheckDTO>> GetByProduct(int productId)
    {
        var items = await _repo.GetByProduct(productId);
        return items.Select(qc => new QualityCheckDTO(
            qc.Id, 
            qc.ProductId, 
            qc.ManufacturingPhaseId, 
            qc.Notes, 
            qc.Status, 
            qc.Severity));
    }

    public async Task<QualityCheckDTO?> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return null;
        
        return new QualityCheckDTO(
            item.Id, 
            item.ProductId, 
            item.ManufacturingPhaseId, 
            item.Notes, 
            item.Status, 
            item.Severity);
    }

    public async Task<QualityCheckDTO> Create(CreateQualityCheckDTO dto)
    {
        // 1. Obter as regras da fase atual
        var phase = await _phaseRepo.GetById(dto.ManufacturingPhaseId);
        if (phase == null) 
            throw new KeyNotFoundException("Fase de fabrico não encontrada.");

        // 2. Lógica de Decisão Automática usando os pesos das Constantes
        int weightObserved = Severity.GetWeight(dto.Severity);
        int weightMax = Severity.GetWeight(phase.MaxAcceptableSeverity);
        int weightRework = Severity.GetWeight(phase.ReworkSeverity);

        string finalStatus;

        if (weightObserved <= weightMax)
            finalStatus = QualityStatus.Passed;
        else if (weightObserved <= weightRework)
            finalStatus = QualityStatus.Rework;
        else
            finalStatus = QualityStatus.Scrapped;

        // 3. Mapear para o Modelo e Gravar
        var entity = new QualityCheckModel
        {
            ProductId = dto.ProductId,
            ManufacturingPhaseId = dto.ManufacturingPhaseId,
            Severity = dto.Severity,
            Notes = dto.Notes,
            Status = finalStatus // Status decidido pelo sistema
        };

        var created = await _repo.Create(entity);

        return new QualityCheckDTO(
            created.Id, 
            created.ProductId, 
            created.ManufacturingPhaseId, 
            created.Notes, 
            created.Status, 
            created.Severity);
    }
}