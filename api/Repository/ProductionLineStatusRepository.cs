using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por obter o estado atual das linhas de produção
public class ProductionLineStatusRepository : IProductionLineStatusRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public ProductionLineStatusRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Devolve o estado atual de todas as linhas de produção e respetivas workstations
    public async Task<List<ProductionLineStatusDTO>> GetStatusAsync()
    {
        // Executa uma query SQL que reúne informação das linhas de produção,
        // workstations, produtos em produção e fase atual de cada produto.
        return await _context.Database
            .SqlQueryRaw<ProductionLineStatusDTO>(
                """
                SELECT
                    pl.id AS "ProductionLineId",
                    pl.name AS "ProductionLineName",
                    pl.location AS "Location",
                    pl.status AS "LineStatus",
                    w.id AS "WorkstationId",
                    w.type AS "WorkstationType",
                    p.id AS "ProductId",
                    p.serial_number AS "SerialNumber",
                    mp.name AS "CurrentPhase",
                    pp.datetime_ini AS "StartedAt",
                    NULL::timestamp AS "EstimatedFinish",
                    pp.result AS "ProductStatus"
                FROM workstation w
                JOIN production_line pl ON pl.id = w.production_line_id
                LEFT JOIN product_phase pp 
                    ON pp.workstation_id = w.id 
                   AND pp.datetime_end IS NULL
                LEFT JOIN product p ON p.id = pp.product_id
                LEFT JOIN manufacturing_phase mp ON mp.id = pp.manufacturing_phase_id
                ORDER BY pl.id, w.id
                """
            )
            .ToListAsync();
    }
}