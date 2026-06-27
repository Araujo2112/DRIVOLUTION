using System.Text;
using System.Text.Json.Serialization;
using Drivolution.Data;
using Drivolution.Repository;
using Drivolution.Repository.Interface;
using Drivolution.Services;
using Drivolution.Services.Interface;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

Env.Load();

// ── Solução global para DateTime UTC com Npgsql ───────────────────────────────
// Garante que todos os DateTime lidos da BD são tratados como UTC,
// eliminando o erro "Cannot write DateTime with Kind=Unspecified"
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new InvalidOperationException("JWT_ISSUER not set");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException("JWT_AUDIENCE not set");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET not set");

var builder = WebApplication.CreateBuilder(args);

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// --- Controllers ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();

// --- Swagger ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Drivolution API v1", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

// --- Database ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- FIWARE / Orion ---

// --- Services ---
builder.Services.AddScoped<IClientOrderService, ClientOrderService>();
builder.Services.AddScoped<IProductPhaseService, ProductPhaseService>();
builder.Services.AddScoped<ILocalizationHistoryService, LocalizationHistoryService>();
builder.Services.AddScoped<IQualityCheckService, QualityCheckService>();
builder.Services.AddScoped<IManufacturingOrderService, ManufacturingOrderService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddHostedService<AlertBackgroundService>();
builder.Services.AddScoped<IWipDashboardService, WipDashboardService>();
builder.Services.AddScoped<IEtaPredictionService, EtaPredictionService>();
builder.Services.AddSingleton<IPhaseTimeWeightCalculator, PhaseTimeWeightCalculator>();
builder.Services.AddScoped<ICarModelEtaSimulationService, CarModelEtaSimulationService>();
builder.Services.AddScoped<IProductionLineStatusService, ProductionLineStatusService>();
builder.Services.AddScoped<IProductTimelineService, ProductTimelineService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IModelTrainingService, ModelTrainingService>();
builder.Services.AddHostedService<MlRetrainBackgroundService>();

// --- Repositories ---
builder.Services.AddScoped<IProductionLineRepository, ProductionLineRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IWorkstationRepository, WorkstationRepository>();
builder.Services.AddScoped<IWorkstationStatusRepository, WorkstationStatusRepository>();
builder.Services.AddScoped<IWorkstationAllocationRepository, WorkstationAllocationRepository>();
builder.Services.AddScoped<ISupportRepository, SupportRepository>();
builder.Services.AddScoped<ILocalizationHistoryRepository, LocalizationHistoryRepository>();
builder.Services.AddScoped<ISupportedProductRepository, SupportedProductRepository>();
builder.Services.AddScoped<ICarModelRepository, CarModelRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IConfigRepository, ConfigRepository>();
builder.Services.AddScoped<IManufacturingPhaseRepository, ManufacturingPhaseRepository>();
builder.Services.AddScoped<IPhaseSequenceRepository, PhaseSequenceRepository>();
builder.Services.AddScoped<IClientOrderRepository, ClientOrderRepository>();
builder.Services.AddScoped<IManufacturingOrderRepository, ManufacturingOrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductPhaseRepository, ProductPhaseRepository>();
builder.Services.AddScoped<IQualityCheckRepository, QualityCheckRepository>();
builder.Services.AddScoped<IProductConfigRepository, ProductConfigRepository>();
builder.Services.AddScoped<IConfigOptionRepository, ConfigOptionRepository>();
builder.Services.AddScoped<IProductTimelineRepository, ProductTimelineRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IWipDashboardRepository, WipDashboardRepository>();
builder.Services.AddScoped<IProductionLineStatusRepository, ProductionLineStatusRepository>();
builder.Services.AddScoped<IPhaseTimeCoefficientRepository, PhaseTimeCoefficientRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// --- JWT Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

var app = builder.Build();

app.UseCors("AllowVueApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Drivolution API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();