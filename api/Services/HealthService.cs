using ApiTexPact.DTO;
using Npgsql;

namespace ApiTexPact.Services;

public interface IHealthService
{
    Task<List<HealthHeartRateDTO>> GetHeartRate(string entityId, DateTime? startTime = null, DateTime? endTime = null);
    Task<HealthStepsDTO> GetTotalSteps(string entityId, DateTime startTime, DateTime endTime);
    Task<int> CalculateActiveTime(string entityId, DateTime startTime, DateTime endTime, int heartRateThreshold = 100);
}

public class HealthService : IHealthService
{
    private readonly string _connectionString;

    public HealthService(IConfiguration configuration)
    {
        var crateHost = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new InvalidOperationException();
        var cratePort = Environment.GetEnvironmentVariable("DB_PORT") ?? throw new InvalidOperationException();
        var crateName = Environment.GetEnvironmentVariable("DB_DEVICEDATA_DB") ?? throw new InvalidOperationException();
        var crateUser = Environment.GetEnvironmentVariable("DB_USER") ?? throw new InvalidOperationException();
        var cratePassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
        
        _connectionString = $"Host={crateHost};" +
                            $"Port={cratePort};" +
                            $"Username={crateUser};" +
                            $"Password={cratePassword};" +
                            $"Database={crateName}";
    }
    
    public async Task<List<HealthHeartRateDTO>> GetHeartRate(string entityId, DateTime? startTime = null, DateTime? endTime = null)
    {
        var result = new List<HealthHeartRateDTO>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var smartwatchId = $"urn:ngsi-ld:Device:smartwatch{entityId}";

        var query = """
            SELECT time_index, heartrate FROM mtfactory.etdevice
            WHERE entity_id=@entityId
            AND heartrate IS NOT NULL
        """;
        
        if (startTime.HasValue)
            query += " AND time_index >= @startTime";
        if (endTime.HasValue)
            query += " AND time_index <= @endTime";
        
        query += " ORDER BY time_index DESC";

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@entityId", smartwatchId);
        if (startTime.HasValue)
            command.Parameters.AddWithValue("@startTime", startTime.Value);
        if (endTime.HasValue)
            command.Parameters.AddWithValue("@endTime", endTime.Value);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new HealthHeartRateDTO
            {
                TimeIndex = reader.GetDateTime(0),
                HeartRate = reader.GetInt32(1)
            });
        }

        return result;
    }

    public async Task<HealthStepsDTO> GetTotalSteps(string entityId, DateTime startTime,
        DateTime endTime)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var smartwatchId = $"urn:ngsi-ld:Device:smartwatch{entityId}";
        
        const string query = """
            WITH steps_data AS (
                SELECT
                    time_index,
                    steps,
                    LAG(steps, 1) OVER (ORDER BY time_index) AS prev_steps
                FROM mtfactory.etdevice
                WHERE steps IS NOT NULL
                  AND time_index BETWEEN @startTime AND @endTime
                  AND entity_id=@smartwatchId
            ),
                 steps_with_deltas AS (
                     SELECT
                         time_index,
                         steps,
                         CASE
                             WHEN prev_steps IS NULL THEN 0
                             WHEN steps < 100 THEN steps
                             WHEN steps >= prev_steps THEN steps - prev_steps
                             ELSE 0
                             END AS delta_steps
                     FROM steps_data
                 )
            SELECT
                COALESCE(SUM(delta_steps), 0) AS total_steps
            FROM steps_with_deltas;
        """;

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@smartwatchId", smartwatchId);
        command.Parameters.AddWithValue("@startTime", startTime);
        command.Parameters.AddWithValue("@endTime", endTime);

        var result = 0;
        
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result = reader.GetInt32(0);
        }

        return new HealthStepsDTO{
            steps = result
        };
    }
    
    public async Task<int> CalculateActiveTime(string entityId, DateTime startTime, DateTime endTime, int heartRateThreshold = 100)
    {
        var heartRates = await GetHeartRate(entityId, startTime, endTime);
        var totalSteps = await GetTotalSteps(entityId, startTime, endTime);

        if (!heartRates.Any())
            return 0;

        var sortedData = heartRates.OrderBy(d => d.TimeIndex).ToList();
        DateTime previousTime = startTime;
        TimeSpan totalActiveTime = TimeSpan.Zero;
        bool isActive = false;
        
        const int ACTIVITY_WINDOW_MINUTES = 5;
        const int MIN_STEPS_PER_WINDOW = 50;
        DateTime windowStart = startTime;
        int windowSteps = 0;

        foreach (var record in sortedData)
        {
            DateTime currentTime = record.TimeIndex;
            TimeSpan timeSinceLastReading = currentTime - previousTime;
            
            if (currentTime - windowStart >= TimeSpan.FromMinutes(ACTIVITY_WINDOW_MINUTES))
            {
                windowStart = currentTime;
                windowSteps = 0;
            }
            
            double timeRatio = timeSinceLastReading.TotalMinutes / (endTime - startTime).TotalMinutes;
            windowSteps += (int)(totalSteps.steps * timeRatio);

            bool isActiveByHeartRate = record.HeartRate > heartRateThreshold;
            bool isActiveBySteps = windowSteps >= MIN_STEPS_PER_WINDOW;
            
            bool currentlyActive = isActiveByHeartRate || isActiveBySteps;
            
            if (currentlyActive && !isActive)
            {
                totalActiveTime += timeSinceLastReading / 2;
            }
            else if (currentlyActive && isActive)
            {
                totalActiveTime += timeSinceLastReading;
            }
            else if (!currentlyActive && isActive)
            {
                totalActiveTime += timeSinceLastReading / 2;
            }

            isActive = currentlyActive;
            previousTime = currentTime;
        }
        
        if (isActive)
        {
            TimeSpan finalInterval = endTime - previousTime;
            totalActiveTime += finalInterval / 2;
        }
        
        return (int)totalActiveTime.TotalMinutes;
    }
}