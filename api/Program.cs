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

// Carrega as variáveis guardadas no ficheiro .env
Env.Load();

// Mantém o comportamento antigo do Npgsql no tratamento de datas e horas.
// É usado para evitar problemas de compatibilidade com timestamps.
AppContext.SetSwitch(
    "Npgsql.EnableLegacyTimestampBehavior",
    true
);

// Obtém os dados necessários para configurar os tokens JWT.
// Se alguma variável não existir, a aplicação lança um erro e não inicia.
var jwtIssuer =
    Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? throw new InvalidOperationException("JWT_ISSUER not set");

var jwtAudience =
    Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? throw new InvalidOperationException("JWT_AUDIENCE not set");

var jwtSecret =
    Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET not set");

// Cria o objeto usado para configurar e construir a aplicação
var builder = WebApplication.CreateBuilder(args);

// --- CORS ---
// Permite que o frontend Vue, que está noutro endereço,
// consiga comunicar com esta API.
builder.Services.AddCors(options =>
{
    // Cria uma política de CORS chamada "AllowVueApp"
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy
            // Apenas este endereço pode fazer pedidos à API
            .WithOrigins("http://localhost:5173")

            // Permite métodos como GET, POST, PUT e DELETE
            .AllowAnyMethod()

            // Permite cabeçalhos como Authorization e Content-Type
            .AllowAnyHeader();
    });
});

// --- Controllers ---
// Adiciona suporte aos controllers da API
builder.Services.AddControllers()

    // Configura a conversão dos objetos C# para JSON
    .AddJsonOptions(options =>
    {
        // Evita ciclos infinitos em relações entre Models.
        // Exemplo: Product -> ManufacturingOrder -> Products -> ...
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;

        // Não envia para o frontend propriedades cujo valor seja null
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
    });

// Regista o HttpClient para permitir pedidos HTTP a serviços externos,
// como o serviço de Machine Learning
builder.Services.AddHttpClient();

// Permite ao Swagger descobrir os endpoints da API
builder.Services.AddEndpointsApiExplorer();

// --- Swagger ---
// Configura a documentação e a interface de testes da API
builder.Services.AddSwaggerGen(c =>
{
    // Define o nome e a versão da documentação
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Drivolution API v1",
            Version = "v1"
        }
    );

    // Diz ao Swagger que a API utiliza autenticação Bearer com JWT
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            // O token é enviado no cabeçalho do pedido
            In = ParameterLocation.Header,

            Description = "Please enter a valid token",

            // Nome do cabeçalho HTTP
            Name = "Authorization",

            // Tipo de autenticação HTTP
            Type = SecuritySchemeType.Http,

            // Formato do token
            BearerFormat = "JWT",

            // Esquema usado: Bearer
            Scheme = "Bearer"
        }
    );

    // Aplica a autenticação Bearer aos endpoints apresentados no Swagger
    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
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

                // Não são necessários scopes adicionais
                []
            }
        }
    );
});

// --- Database ---
// Regista o ApplicationDbContext no sistema de injeção de dependências
builder.Services.AddDbContext<ApplicationDbContext>(options =>

    // Indica que é usada uma base de dados PostgreSQL
    // e obtém a connection string chamada "DefaultConnection"
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    )
);

// --- Services ---
// Regista cada interface e a respetiva implementação.
// Assim, quando uma classe pede uma interface,
// o ASP.NET sabe que classe concreta deve criar.

builder.Services.AddScoped<
    IClientOrderService,
    ClientOrderService
>();

builder.Services.AddScoped<
    IUserService,
    UserService
>();

builder.Services.AddScoped<
    IProductPhaseService,
    ProductPhaseService
>();

builder.Services.AddScoped<
    ILocalizationHistoryService,
    LocalizationHistoryService
>();

builder.Services.AddScoped<
    IQualityCheckService,
    QualityCheckService
>();

builder.Services.AddScoped<
    IManufacturingOrderService,
    ManufacturingOrderService
>();

builder.Services.AddScoped<
    IAlertService,
    AlertService
>();

// Regista um serviço que trabalha automaticamente em segundo plano
builder.Services.AddHostedService<AlertBackgroundService>();

builder.Services.AddScoped<
    IWipDashboardService,
    WipDashboardService
>();

builder.Services.AddScoped<
    IEtaPredictionService,
    EtaPredictionService
>();

builder.Services.AddScoped<
    IProductService,
    ProductService
>();

builder.Services.AddScoped<
    IClientPortalRepository,
    ClientPortalRepository
>();

builder.Services.AddScoped<
    IClientPortalService,
    ClientPortalService
>();

// Singleton: é criada apenas uma instância durante toda a aplicação
builder.Services.AddSingleton<
    IPhaseTimeWeightCalculator,
    PhaseTimeWeightCalculator
>();

builder.Services.AddScoped<
    ICarModelEtaSimulationService,
    CarModelEtaSimulationService
>();

builder.Services.AddScoped<
    IProductionLineStatusService,
    ProductionLineStatusService
>();

builder.Services.AddScoped<
    IProductTimelineService,
    ProductTimelineService
>();

builder.Services.AddScoped<
    IAuthService,
    AuthService
>();

// É criada apenas uma instância deste service
builder.Services.AddSingleton<
    IModelTrainingService,
    ModelTrainingService
>();

// Serviço em segundo plano responsável pelo retreino do modelo
builder.Services.AddHostedService<MlRetrainBackgroundService>();

builder.Services.AddScoped<
    IAnalyticsService,
    AnalyticsService
>();

builder.Services.AddScoped<
    IWorkstationPresenceService,
    WorkstationPresenceService
>();

builder.Services.AddScoped<
    IAuditService,
    AuditService
>();

// --- Repositories ---
// Regista cada interface de repository
// e a respetiva implementação concreta.

builder.Services.AddScoped<
    IProductionLineRepository,
    ProductionLineRepository
>();

builder.Services.AddScoped<
    IResourceRepository,
    ResourceRepository
>();

builder.Services.AddScoped<
    IWorkstationRepository,
    WorkstationRepository
>();

builder.Services.AddScoped<
    IWorkstationStatusRepository,
    WorkstationStatusRepository
>();

builder.Services.AddScoped<
    IWorkstationAllocationRepository,
    WorkstationAllocationRepository
>();

builder.Services.AddScoped<
    ISupportRepository,
    SupportRepository
>();

builder.Services.AddScoped<
    ILocalizationHistoryRepository,
    LocalizationHistoryRepository
>();

builder.Services.AddScoped<
    ISupportedProductRepository,
    SupportedProductRepository
>();

builder.Services.AddScoped<
    ICarModelRepository,
    CarModelRepository
>();

builder.Services.AddScoped<
    IMaterialRepository,
    MaterialRepository
>();

builder.Services.AddScoped<
    IConfigRepository,
    ConfigRepository
>();

builder.Services.AddScoped<
    IManufacturingPhaseRepository,
    ManufacturingPhaseRepository
>();

builder.Services.AddScoped<
    IPhaseSequenceRepository,
    PhaseSequenceRepository
>();

builder.Services.AddScoped<
    IClientOrderRepository,
    ClientOrderRepository
>();

builder.Services.AddScoped<
    IManufacturingOrderRepository,
    ManufacturingOrderRepository
>();

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository
>();

builder.Services.AddScoped<
    IProductPhaseRepository,
    ProductPhaseRepository
>();

builder.Services.AddScoped<
    IQualityCheckRepository,
    QualityCheckRepository
>();

builder.Services.AddScoped<
    IProductConfigRepository,
    ProductConfigRepository
>();

builder.Services.AddScoped<
    IConfigOptionRepository,
    ConfigOptionRepository
>();

builder.Services.AddScoped<
    IProductTimelineRepository,
    ProductTimelineRepository
>();

builder.Services.AddScoped<
    IAlertRepository,
    AlertRepository
>();

builder.Services.AddScoped<
    IWipDashboardRepository,
    WipDashboardRepository
>();

builder.Services.AddScoped<
    IProductionLineStatusRepository,
    ProductionLineStatusRepository
>();

builder.Services.AddScoped<
    IPhaseTimeCoefficientRepository,
    PhaseTimeCoefficientRepository
>();

builder.Services.AddScoped<
    IUserRepository,
    UserRepository
>();

builder.Services.AddScoped<
    IAnalyticsRepository,
    AnalyticsRepository
>();

builder.Services.AddScoped<
    IWorkstationPresenceRepository,
    WorkstationPresenceRepository
>();

builder.Services.AddScoped<
    IAuditRepository,
    AuditRepository
>();

// Apesar de estar nesta secção, isto é um service.
// O NotificationService usa diretamente o DbContext.
builder.Services.AddScoped<
    INotificationService,
    NotificationService
>();

// --- JWT Authentication ---
// Configura o sistema de autenticação da API
builder.Services.AddAuthentication(options =>
{
    // Define JWT Bearer como o método normal para autenticar pedidos
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    // Define JWT Bearer como o método usado quando
    // um utilizador não está autenticado
    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Define as regras usadas para validar o token recebido
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            // Confirma quem emitiu o token
            ValidateIssuer = true,

            // Confirma para que aplicação o token foi criado
            ValidateAudience = true,

            // Confirma se o token ainda não expirou
            ValidateLifetime = true,

            // Confirma se a assinatura do token é válida
            ValidateIssuerSigningKey = true,

            // Emissor esperado
            ValidIssuer = jwtIssuer,

            // Destinatário esperado
            ValidAudience = jwtAudience,

            // Chave secreta usada para validar a assinatura do token
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)
                )
        };
});

// Constrói a aplicação com todas as configurações anteriores
var app = builder.Build();

// Ativa a política CORS criada anteriormente
app.UseCors("AllowVueApp");

// O Swagger só fica disponível no ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    // Gera o documento JSON do Swagger
    app.UseSwagger();

    // Ativa a interface visual do Swagger
    app.UseSwaggerUI(c =>
    {
        // Indica onde está o documento da API
        c.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Drivolution API v1"
        );

        // O Swagger fica disponível em /swagger
        c.RoutePrefix = "swagger";
    });
}

// Ativa o sistema de rotas da aplicação
app.UseRouting();

// Tenta identificar o utilizador através do token JWT
app.UseAuthentication();

// Verifica se o utilizador tem autorização
// para aceder ao endpoint pedido
app.UseAuthorization();

// Executa o middleware que verifica
// se o utilizador é obrigado a alterar a password
app.UseMiddleware<
    Drivolution.Middleware.MustChangePasswordMiddleware
>();

// Liga as rotas HTTP aos controllers
app.MapControllers();

// Inicia a aplicação e fica à espera de pedidos HTTP
app.Run();