using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por obter os dados apresentados no dashboard WIP
public class WipDashboardRepository : IWipDashboardRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public WipDashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Obtém todos os produtos que estão atualmente em produção
    public async Task<List<WipItemDTO>> GetInProgressAsync()
    {
        // Executa uma query SQL que reúne a informação do produto,
        // da fase atual, da workstation e da linha de produção
        return await _context.Database
            .SqlQueryRaw<WipItemDTO>(
                """
                SELECT
                    p.id AS "ProductId",
                    p.model_id AS "ModelId",
                    p.serial_number AS "SerialNumber",
                    pl.id AS "ProductionLineId",
                    pl.name AS "ProductionLineName",
                    w.id AS "WorkstationId",
                    w.type AS "Workstation",
                    mp.name AS "CurrentPhase",
                    mp.estimated_duration AS "EstimatedDuration",
                    mp.time_threshold_pct AS "TimeThresholdPct",
                    pp.datetime_ini AS "StartedAt",
                    pp.datetime_end AS "EndedAt",
                    pp.result AS "Result",

                    -- Todos os produtos desta query estão em produção
                    'in_progress' AS "WipStatus",

                    -- Calcula há quantos segundos o produto está na fase atual
                    EXTRACT(EPOCH FROM (NOW() - pp.datetime_ini))::INT AS "ElapsedSeconds",

                    -- Estes valores serão preenchidos posteriormente pelo Service
                    NULL::INT AS "PredictedPhaseDurationSeconds",
                    FALSE AS "PredictedPhaseDurationIsMl"

                FROM product_phase pp

                -- Obtém o produto associado à fase
                JOIN product p ON p.id = pp.product_id

                -- Obtém os dados da fase de fabrico
                JOIN manufacturing_phase mp ON mp.id = pp.manufacturing_phase_id

                -- Obtém a workstation onde o produto se encontra
                JOIN workstation w ON w.id = pp.workstation_id

                -- Obtém a linha de produção da workstation
                JOIN production_line pl ON pl.id = w.production_line_id

                -- Uma fase sem data de fim ainda está em execução
                WHERE pp.datetime_end IS NULL

                -- Apresenta primeiro as fases iniciadas mais recentemente
                ORDER BY pp.datetime_ini DESC
                """
            )
            .ToListAsync();
    }

    // Obtém os produtos que estão à espera de iniciar ou continuar a produção
    public async Task<List<WaitingItemDTO>> GetWaitingAsync()
    {
        // A query utiliza várias CTEs para preparar diferentes conjuntos
        // de informação antes de construir o resultado final
        return await _context.Database
            .SqlQueryRaw<WaitingItemDTO>(
                """
                -- Produtos que têm atualmente uma fase aberta
                WITH active_phase AS (
                    SELECT DISTINCT product_id
                    FROM product_phase
                    WHERE datetime_end IS NULL
                ),

                -- Última fase concluída por cada produto
                last_phase AS (
                    SELECT DISTINCT ON (pp.product_id)
                        pp.product_id,
                        pp.manufacturing_phase_id,
                        mp.name AS phase_name,
                        pp.datetime_end
                    FROM product_phase pp
                    JOIN manufacturing_phase mp ON mp.id = pp.manufacturing_phase_id
                    WHERE pp.datetime_end IS NOT NULL
                    ORDER BY pp.product_id, pp.datetime_end DESC
                ),

                -- Suporte atualmente associado a cada produto
                current_support AS (
                    SELECT DISTINCT ON (sp.product_id)
                        sp.product_id,
                        sp.support_id,
                        s.rfid_tag,
                        s.type AS support_type
                    FROM supported_product sp
                    JOIN support s ON s.id = sp.support_id
                    WHERE sp.datetime_end IS NULL
                    ORDER BY sp.product_id, sp.datetime_ini DESC
                ),

                -- Localização atual de cada suporte
                current_location AS (
                    SELECT DISTINCT ON (lh.support_id)
                        lh.support_id,
                        lh.workstation_id,
                        w.type AS workstation,
                        pl.id AS production_line_id,
                        pl.name AS production_line_name
                    FROM localization_history lh
                    JOIN workstation w ON w.id = lh.workstation_id
                    JOIN production_line pl ON pl.id = w.production_line_id
                    WHERE lh.datetime_end IS NULL
                    ORDER BY lh.support_id, lh.datetime_ini DESC
                )

                SELECT
                    p.id AS "ProductId",
                    p.serial_number AS "SerialNumber",
                    cm.name AS "ModelName",
                    mo.id AS "ManufacturingOrderId",
                    mo.manufacturing_order_number AS "ManufacturingOrderNumber",
                    mo.status AS "ManufacturingOrderStatus",
                    cs.support_id AS "SupportId",
                    cs.rfid_tag AS "RfidTag",
                    cs.support_type AS "SupportType",
                    cl.production_line_id AS "ProductionLineId",
                    cl.production_line_name AS "ProductionLineName",
                    cl.workstation_id AS "WorkstationId",
                    cl.workstation AS "Workstation",
                    lp.phase_name AS "LastPhase",
                    lp.datetime_end AS "LastPhaseEndedAt",
                    next_mp.name AS "NextPhase",

                    -- Define a razão pela qual o produto está em espera
                    CASE
                        -- Ainda não tem skid associado
                        WHEN cs.support_id IS NULL THEN 'waiting_support'

                        -- Já tem skid, mas ainda não entrou numa linha
                        WHEN cl.workstation_id IS NULL THEN 'waiting_line'

                        -- Está entre fases, à espera da próxima
                        ELSE 'waiting_next_phase'
                    END AS "QueueReason"

                FROM product p

                -- Obtém a ordem de fabrico associada ao produto
                JOIN manufacturing_order mo ON mo.id = p.manufacturing_order_id

                -- Obtém o modelo do veículo
                JOIN model cm ON cm.id = p.model_id

                -- Verifica se o produto tem uma fase ativa
                LEFT JOIN active_phase ap ON ap.product_id = p.id

                -- Obtém a última fase concluída
                LEFT JOIN last_phase lp ON lp.product_id = p.id

                -- Procura a posição da última fase na sequência do modelo
                LEFT JOIN phase_sequence last_ps
                    ON last_ps.model_id = p.model_id
                   AND last_ps.manufacturing_phase_id = lp.manufacturing_phase_id

                -- Procura a próxima fase da sequência
                LEFT JOIN phase_sequence next_ps
                    ON next_ps.model_id = p.model_id
                   AND next_ps."order" = COALESCE(last_ps."order", 0) + 1

                -- Obtém o nome da próxima fase
                LEFT JOIN manufacturing_phase next_mp
                    ON next_mp.id = next_ps.manufacturing_phase_id

                -- Obtém o suporte atual
                LEFT JOIN current_support cs ON cs.product_id = p.id

                -- Obtém a localização atual do suporte
                LEFT JOIN current_location cl ON cl.support_id = cs.support_id

                -- Mantém apenas produtos sem fase ativa
                WHERE ap.product_id IS NULL

                  -- Ignora produtos cuja ordem já está concluída
                  AND COALESCE(mo.status, '') <> 'completed'

                -- Ordena primeiro os que não têm suporte,
                -- depois os que não estão numa linha e, por fim,
                -- os que aguardam a próxima fase
                ORDER BY
                    CASE
                        WHEN cs.support_id IS NULL THEN 1
                        WHEN cl.workstation_id IS NULL THEN 2
                        ELSE 3
                    END,
                    mo.start_date,
                    p.id
                """
            )
            .ToListAsync();
    }

    // Conta o número total de produtos concluídos
    public async Task<int> GetCompletedCountAsync()
    {
        // Executa uma query que conta os produtos pertencentes
        // a ordens de fabrico com estado "completed"
        var result = await _context.Database
            .SqlQueryRaw<CountDTO>(
                """
                SELECT COUNT(DISTINCT p.id)::INT AS "Value"
                FROM product p
                JOIN manufacturing_order mo ON mo.id = p.manufacturing_order_id
                WHERE mo.status = 'completed'
                """
            )
            .FirstAsync();

        // Devolve apenas o valor da contagem
        return result.Value;
    }
}