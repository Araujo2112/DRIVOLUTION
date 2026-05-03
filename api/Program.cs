using System.Net;
using System.Net.Security;
using System.Text;
using System.Text.Json.Serialization;
using ApiTexPact.Data;
using ApiTexPact.Repository;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Repository.Interface.Client;
using ApiTexPact.Repository.Interface.ManufacturingOrder;
using ApiTexPact.Repository.Interface.ManufacturingOrderHistory;
using ApiTexPact.Repository.Interface.ManufacturingOrderPhase;
using ApiTexPact.Repository.Interface.ManufacturingPhase;
using ApiTexPact.Repository.Interface.ManufacturingProcessPhase;
using ApiTexPact.Repository.Interface.Prediction;
using ApiTexPact.Repository.Interface.Product;
using ApiTexPact.Repository.Interface.ProductLot;
using ApiTexPact.Service;
using ApiTexPact.Service.Interface;
using ApiTexPact.Services;
using ApiTexPact.Services.ArrowFlightClient;
using ApiTexPact.Services.ArrowFlightClient.Interfaces;
using ApiTexPact.Services.Interface;
using ApiTexPact.Services.Interface.Client;
using ApiTexPact.Services.Interface.Containers;
using ApiTexPact.Services.Interface.ManufacturingOrder;
using ApiTexPact.Services.Interface.ManufacturingOrderHistory;
using ApiTexPact.Services.Interface.ManufacturingOrderPhase;
using ApiTexPact.Services.Interface.ManufacturingPhase;
using ApiTexPact.Services.Interface.ManufacturingProcessPhase;
using ApiTexPact.Services.Interface.Product;
using ApiTexPact.Services.Interface.ProductLot;
using ApiTexPact.Services.PlantFloorSection;
using ApiTexPact.Services.Prediction;
using ApiTexPact.Services.Prediction.Interfaces;
using ApiTexPact.Services.RawMaterial;
using ApiTexPact.Services.RawMaterial.Interfaces.ItemInContainer;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Texpact;
using CheckpointService = Texpact.CheckpointService;
using ClientService = Texpact.ClientService;
using ItemLocalizationService = Texpact.ItemLocalizationService;
using ManufacturingOrderService = Texpact.ManufacturingOrderService;
using PlantFloorSectionService = Texpact.PlantFloorSectionService;
using ProductService = Texpact.ProductService;
using RawMaterialService = Texpact.RawMaterialService;

/*

*/

Env.Load();

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new InvalidOperationException();
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException();
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException();


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<OrionService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FactoryProject v1", Version = "v1" });
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

//ChatBot

builder.Services.AddHttpClient("ollama", c =>
{
    c.BaseAddress = new Uri("http://localhost:11434/api/");
    c.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddGrpcClient<ClientService.ClientServiceClient>(opts =>
{
    opts.Address = new Uri("http://localhost:50051");
});

builder.Services.AddGrpcClient<ContainerService.ContainerServiceClient>(opts =>
{
    opts.Address = new Uri("http://localhost:50051");
});

builder.Services.AddGrpcClient<RawMaterialService.RawMaterialServiceClient>(opts =>
{
    opts.Address = new Uri("http://localhost:50051");
});

builder.Services.AddGrpcClient<ProductService.ProductServiceClient>(opts =>
{
    opts.Address = new Uri("http://localhost:50051");
});

builder.Services.AddGrpcClient<PlantFloorSectionService.PlantFloorSectionServiceClient>(opts =>
{
    opts.Address = new Uri("http://localhost:50051");
});

builder.Services.AddGrpcClient<CheckpointService.CheckpointServiceClient>(opts =>
{
    opts.Address = new Uri("http://localhost:50051");
});

builder.Services.AddGrpcClient<ContainerLocalizationService.ContainerLocalizationServiceClient>(opts =>
{
    opts.Address = new Uri("http://localhost:50051");
});

builder.Services.AddGrpcClient<ManufacturingOrderService.ManufacturingOrderServiceClient>(opts =>
{
    opts.Address = new Uri("http://localhost:50051");
});

builder.Services.AddGrpcClient<ItemLocalizationService.ItemLocalizationServiceClient>(opts =>
{
    opts.Address = new Uri("http://localhost:50051");
});

builder.Services.AddGrpcClient<MlpPredictionService.MlpPredictionServiceClient>(opts =>
{
    opts.Address = new Uri("http://localhost:50051");
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


//Container
builder.Services.AddScoped<IContainerRepository, ContainerRepository>();
builder.Services.AddScoped<IContainerService, ApiTexPact.Services.FactoryElements.ContainerService>();

//RawMaterials
builder.Services.AddScoped<IRawMaterialRepository, RawMaterialRepository>();
builder.Services.AddScoped<IRawMaterialService, ApiTexPact.Service.RawMaterialService>();


//ItemOfRawMaterial
builder.Services.AddScoped<IItemOfRawMaterialRepository, ItemOfRawMaterialRepository>();
builder.Services.AddScoped<IItemOfRawMaterialService, ItemOfRawMaterialService>();

//ItemInContainer
builder.Services.AddScoped<IItemInContainerRepository, ItemInContainerRepository>();
builder.Services.AddScoped<IItemInContainerService, ItemInContainerService>();


//ItemLocalizationHistory
builder.Services.AddScoped<IItemLocalizationRepository, ItemLocalizationRepository>();
builder.Services.AddScoped<IItemLocalizationService, ApiTexPact.Service.ItemLocalizationService>();

//Checkpoint
builder.Services.AddScoped<ICheckpointRepository, CheckpointRepository>();
builder.Services.AddScoped<ICheckpointService, ApiTexPact.Services.CheckpointService>();

//PlantFloorSection
builder.Services.AddScoped<IPlantFloorSectionRepository, PlantFloorSectionRepository>();
builder.Services.AddScoped<IPlantFloorSectionService, ApiTexPact.Services.PlantFloorSection.PlantFloorSectionService>();

//LotOfRawMaterial
builder.Services.AddScoped<ILotOfRawMaterialRepository, LotOfRawMaterialRepository>();
builder.Services.AddScoped<ILotOfRawMaterialService, LotOfRawMaterialService>();

//ContainerLocalization
builder.Services.AddScoped<IContainerLocalizationRepository, ContainerLocalizationRepository>();
builder.Services
    .AddScoped<IContainerLocalizationService, ApiTexPact.Services.Containers.ContainerLocalizationService>();

//Client
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientService, ApiTexPact.Services.ClientService>();

//ManufacturingOrderHistory
builder.Services.AddScoped<IManufacturingOrderHistoryRepository, ManufacturingOrderHistoryRepository>();
builder.Services.AddScoped<IManufacturingOrderHistoryService, ManufacturingOrderHistoryService>();

//ManufacturingOrderPhase
builder.Services.AddScoped<IManufacturingOrderPhaseRepository, ManufacturingOrderPhaseRepository>();
builder.Services.AddScoped<IManufacturingOrderPhaseService, ManufacturingOrderPhaseService>();

//ManufacturingOrder
builder.Services.AddScoped<IManufacturingOrderRepository, ManufacturingOrderRepository>();
builder.Services.AddScoped<IManufacturingOrderService, ApiTexPact.Services.ManufacturingOrderService>();

//ManufacturingPhase
builder.Services.AddScoped<IManufacturingPhaseRepository, ManufacturingPhaseRepository>();
builder.Services.AddScoped<IManufacturingPhaseService, ManufacturingPhaseService>();

//ManufacturingProcessPhase
builder.Services.AddScoped<IManufacturingProcessPhaseRepository, ManufacturingProcessPhaseRepository>();
builder.Services.AddScoped<IManufacturingProcessPhaseService, ManufacturingProcessPhaseService>();

//ManufacturingProcess
builder.Services.AddScoped<IManufacturingProcessRepository, ManufacturingProcessRepository>();
builder.Services.AddScoped<IManufacturingProcessService, ManufacturingProcessService>();

//Product
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ApiTexPact.Services.ProductService>();

//ProductLot
builder.Services.AddScoped<IProductLotRepository, ProductLotRepository>();
builder.Services.AddScoped<IProductLotService, ProductLotService>();

//Notification
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// AI
/*
builder.Services.AddSingleton<IArrowFlightClientService, ArrowFlightClientService>();
builder.Services.AddScoped<IPredictionRepository, PredictionRepository>();
builder.Services.AddScoped<IPredictionService, PredictionService>();
*/

// Only DeV Usage !
builder.Services
    .AddHttpClient<IQuicPredictionClientService, QuicPredictionClientService>(c =>
    {
        c.BaseAddress = new Uri("https://localhost:4433/");
        c.DefaultRequestVersion = HttpVersion.Version30;
        c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler
        {
            SslOptions =
            {
                ApplicationProtocols = new List<SslApplicationProtocol>
                {
                    SslApplicationProtocol.Http3 
                },
                RemoteCertificateValidationCallback =
                    (_, _, _, _) => true 
            }
        });


builder.Services.AddScoped<IPredictionRepository, PredictionRepository>();


builder.Services.AddScoped<IPredictionService, PredictionService>();


builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IHealthService, HealthService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FactoryProject v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();