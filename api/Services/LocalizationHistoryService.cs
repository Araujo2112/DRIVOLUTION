using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Drivolution.Models.Constants;

namespace Drivolution.Services;

// Service responsável pela gestão do histórico de localização dos suportes (skids)
public class LocalizationHistoryService : ILocalizationHistoryService
{
    // Repository responsável pelo acesso aos registos de localização
    private readonly ILocalizationHistoryRepository _repo;

    // O ASP.NET injeta automaticamente o repository
    public LocalizationHistoryService(ILocalizationHistoryRepository repo)
    {
        _repo = repo;
    }

    // Devolve todo o histórico de localizações de um suporte
    public async Task<IEnumerable<LocalizationHistoryDTO>> GetBySupport(int supportId)
    {
        // Obtém o histórico do repository
        var items = await _repo.GetBySupport(supportId);

        // Converte cada registo para DTO antes de devolver
        return items.Select(ToDTO);
    }

    // Devolve apenas a localização atual (ativa) de um suporte
    public async Task<LocalizationHistoryDTO?> GetCurrent(int supportId)
    {
        // Procura a localização que ainda não foi terminada
        var item = await _repo.GetCurrentBySupport(supportId);

        // Se não existir devolve null, caso contrário converte para DTO
        return item == null ? null : ToDTO(item);
    }

    // Cria um novo registo de localização
    public async Task<LocalizationHistoryDTO> Create(CreateLocalizationHistoryDTO dto)
    {
        // Fechar a localização ativa antes de abrir uma nova
        var current = await _repo.GetCurrentBySupport(dto.SupportId);

        if (current != null)
        {
            // Regista a data de saída da workstation anterior
            current.DatetimeEnd = DateTime.UtcNow;

            // Marca o registo anterior como concluído
            current.Status = ActiveStatus.Completed;

            // Guarda as alterações
            await _repo.Update(current);
        }

        // Cria o novo registo correspondente à workstation atual
        var entity = new LocalizationHistoryModel
        {
            SupportId = dto.SupportId,
            WorkstationId = dto.WorkstationId,

            // Momento em que o suporte entrou nesta workstation
            DatetimeIni = DateTime.UtcNow,

            // O novo registo começa sempre ativo
            Status = ActiveStatus.Active
        };

        // Guarda o novo registo
        var created = await _repo.Create(entity);

        // Converte para DTO e devolve o resultado
        return ToDTO(created);
    }

    // --- Auxiliar ---
    // Converte um LocalizationHistoryModel para LocalizationHistoryDTO
    private static LocalizationHistoryDTO ToDTO(LocalizationHistoryModel lh) =>
        new(
            lh.Id,
            lh.SupportId,
            lh.WorkstationId,
            lh.Workstation?.Type,
            lh.DatetimeIni,
            lh.DatetimeEnd,
            lh.Status
        );
}