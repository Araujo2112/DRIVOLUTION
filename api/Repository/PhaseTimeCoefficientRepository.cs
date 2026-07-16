using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir os coeficientes utilizados pelo modelo de Machine Learning
public class PhaseTimeCoefficientRepository : IPhaseTimeCoefficientRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public PhaseTimeCoefficientRepository(ApplicationDbContext context) => _context = context;

    // Devolve todos os coeficientes do modelo de Machine Learning
    // O conjunto de dados é pequeno, por isso é carregado de uma só vez
    // e filtrado posteriormente pelo serviço.
    public async Task<IEnumerable<PhaseTimeCoefficientModel>> GetAll() =>
        await _context.Set<PhaseTimeCoefficientModel>().ToListAsync();

    // Devolve a data e hora da última vez que o modelo foi treinado
    public async Task<DateTime?> GetLastTrainedAt() =>
        await _context.Set<PhaseTimeCoefficientModel>()
            .OrderByDescending(c => c.TrainedAt)
            .Select(c => c.TrainedAt)
            .FirstOrDefaultAsync();
}