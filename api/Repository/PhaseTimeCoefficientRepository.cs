using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class PhaseTimeCoefficientRepository : IPhaseTimeCoefficientRepository
{
    private readonly ApplicationDbContext _context;
    public PhaseTimeCoefficientRepository(ApplicationDbContext context) => _context = context;

    // Dataset pequeno (uma linha por opção/linha/modelo/intercepto, por fase) —
    // carregar tudo de uma vez e filtrar em memória no serviço é mais simples
    // e suficientemente rápido do que multiplicar queries.
    public async Task<IEnumerable<PhaseTimeCoefficientModel>> GetAll() =>
        await _context.Set<PhaseTimeCoefficientModel>().ToListAsync();

    public async Task<DateTime?> GetLastTrainedAt() =>
        await _context.Set<PhaseTimeCoefficientModel>()
            .OrderByDescending(c => c.TrainedAt)
            .Select(c => c.TrainedAt)
            .FirstOrDefaultAsync();
}