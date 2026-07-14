using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Implementa o contrato definido em IProductTimelineRepository
public class ProductTimelineRepository : IProductTimelineRepository
{
    // Contexto do Entity Framework usado para aceder à base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o contexto da base de dados
    public ProductTimelineRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Verifica se existe um produto com o ID indicado
    public async Task<bool> ProductExists(int productId) =>
        await _context.Database

            // Executa uma consulta SQL que procura o ID do produto
            .SqlQueryRaw<int>(
                "SELECT id FROM product WHERE id = {0}",
                productId
            )

            // Devolve true se encontrar pelo menos um registo
            .AnyAsync();

    // Verifica se existe um produto com o número de série indicado
    public async Task<bool> ProductExistsBySerial(string serialNumber) =>
        await _context.Database

            // Executa uma consulta SQL para procurar o produto pelo serial number
            .SqlQueryRaw<int>(
                """
                SELECT id
                FROM product
                WHERE serial_number = {0}
                """,
                serialNumber
            )

            // Devolve true se o produto existir
            .AnyAsync();

    // Obtém a timeline de um produto através do seu ID
    public async Task<List<ProductTimelineDTO>> GetTimeline(int productId) =>
        await _context.Database

            // Executa uma consulta SQL e transforma cada linha
            // do resultado num ProductTimelineDTO
            .SqlQueryRaw<ProductTimelineDTO>(
                """
                SELECT

                    -- ID do registo da fase do produto
                    pp.id AS "ProductPhaseId",

                    -- Dados do produto
                    p.id AS "ProductId",
                    p.model_id AS "ModelId",
                    p.serial_number AS "SerialNumber",

                    -- Dados da fase de fabrico
                    mp.id AS "ManufacturingPhaseId",
                    mp.name AS "PhaseName",

                    -- Tipo/nome da workstation
                    w.type AS "Workstation",

                    -- Data de início e fim da fase
                    pp.datetime_ini AS "StartedAt",
                    pp.datetime_end AS "EndedAt",

                    -- Calcula a duração da fase em segundos
                    CASE
                        -- Se a fase ainda não terminou,
                        -- ainda não existe uma duração final
                        WHEN pp.datetime_end IS NULL THEN NULL

                        -- Se terminou, calcula:
                        -- data final - data inicial
                        ELSE EXTRACT(
                            EPOCH FROM (
                                pp.datetime_end -
                                pp.datetime_ini
                            )
                        )::INT
                    END AS "DurationSeconds",

                    -- Resultado e notas da fase
                    pp.result AS "Result",
                    pp.notes AS "Notes",

                    -- A previsão é preenchida posteriormente pelo service
                    NULL::timestamp AS "EstimatedFinish"

                -- A tabela principal é product_phase
                FROM product_phase pp

                -- Junta o produto associado à fase
                JOIN product p
                    ON p.id = pp.product_id

                -- Junta a fase de fabrico
                JOIN manufacturing_phase mp
                    ON mp.id = pp.manufacturing_phase_id

                -- Junta a workstation onde a fase decorreu
                JOIN workstation w
                    ON w.id = pp.workstation_id

                -- Filtra apenas as fases do produto indicado
                WHERE pp.product_id = {0}

                -- Ordena as fases da mais antiga para a mais recente
                ORDER BY pp.datetime_ini
                """,

                // O valor substitui o marcador {0}
                productId
            )

            // Executa a consulta e devolve os resultados numa lista
            .ToListAsync();

    // Obtém a timeline através do número de série do produto
    public async Task<List<ProductTimelineDTO>> GetTimelineBySerial(
        string serialNumber) =>
        await _context.Database
            .SqlQueryRaw<ProductTimelineDTO>(
                """
                SELECT
                    pp.id AS "ProductPhaseId",
                    p.id AS "ProductId",
                    p.model_id AS "ModelId",
                    p.serial_number AS "SerialNumber",
                    mp.id AS "ManufacturingPhaseId",
                    mp.name AS "PhaseName",
                    w.type AS "Workstation",
                    pp.datetime_ini AS "StartedAt",
                    pp.datetime_end AS "EndedAt",

                    -- Calcula a duração da fase em segundos,
                    -- apenas quando a fase já terminou
                    CASE
                        WHEN pp.datetime_end IS NULL THEN NULL
                        ELSE EXTRACT(
                            EPOCH FROM (
                                pp.datetime_end -
                                pp.datetime_ini
                            )
                        )::INT
                    END AS "DurationSeconds",

                    pp.result AS "Result",
                    pp.notes AS "Notes",

                    -- Será preenchido posteriormente pelo service de timeline
                    NULL::timestamp AS "EstimatedFinish"

                FROM product_phase pp

                JOIN product p
                    ON p.id = pp.product_id

                JOIN manufacturing_phase mp
                    ON mp.id = pp.manufacturing_phase_id

                JOIN workstation w
                    ON w.id = pp.workstation_id

                -- Aqui o filtro é feito pelo número de série
                WHERE p.serial_number = {0}

                -- Ordena as fases cronologicamente
                ORDER BY pp.datetime_ini
                """,
                serialNumber
            )
            .ToListAsync();
}