using ApiTexPact.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // --- Infraestrutura ---
    public DbSet<ProductionLineModel> ProductionLines { get; set; }
    public DbSet<ResourceModel> Resources { get; set; }
    public DbSet<WorkstationModel> Workstations { get; set; }
    public DbSet<WorkstationStatusModel> WorkstationStatuses { get; set; }
    public DbSet<WorkstationAllocationModel> WorkstationAllocations { get; set; }
    public DbSet<SupportModel> Supports { get; set; }
    public DbSet<LocalizationHistoryModel> LocalizationHistories { get; set; }

    // --- Modelo de Produto ---
    public DbSet<CarModelModel> CarModels { get; set; }
    public DbSet<MaterialModel> Materials { get; set; }
    public DbSet<ModelMaterialModel> ModelMaterials { get; set; }
    public DbSet<ConfigModel> Configs { get; set; }
    public DbSet<ConfigOptionModel> ConfigOptions { get; set; }
    public DbSet<ManufacturingPhaseModel> ManufacturingPhases { get; set; }
    public DbSet<PhaseSequenceModel> PhaseSequences { get; set; }

    // --- Ordens e Produção ---
    public DbSet<ClientOrderModel> ClientOrders { get; set; }
    public DbSet<ManufacturingOrderModel> ManufacturingOrders { get; set; }
    public DbSet<ProductModel> Products { get; set; }
    public DbSet<ProductPhaseModel> ProductPhases { get; set; }
    public DbSet<QualityCheckModel> QualityChecks { get; set; }
    public DbSet<SupportedProductModel> SupportedProducts { get; set; }
    public DbSet<ProductConfigModel> ProductConfigs { get; set; }

    // --- Alertas ---
    public DbSet<AlertModel> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ProductionLine → Workstation (1:N)
        modelBuilder.Entity<WorkstationModel>()
            .HasOne(w => w.ProductionLine)
            .WithMany(pl => pl.Workstations)
            .HasForeignKey(w => w.ProductionLineId)
            .OnDelete(DeleteBehavior.Restrict);

        // ProductionLine → Support (1:N)
        modelBuilder.Entity<SupportModel>()
            .HasOne(s => s.ProductionLine)
            .WithMany(pl => pl.Supports)
            .HasForeignKey(s => s.ProductionLineId)
            .OnDelete(DeleteBehavior.Restrict);

        // Resource → WorkstationAllocation (1:N)
        modelBuilder.Entity<WorkstationAllocationModel>()
            .HasOne(wa => wa.Resource)
            .WithMany(r => r.WorkstationAllocations)
            .HasForeignKey(wa => wa.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Workstation → WorkstationAllocation (1:N)
        modelBuilder.Entity<WorkstationAllocationModel>()
            .HasOne(wa => wa.Workstation)
            .WithMany(w => w.WorkstationAllocations)
            .HasForeignKey(wa => wa.WorkstationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Workstation → WorkstationStatus (1:N)
        modelBuilder.Entity<WorkstationStatusModel>()
            .HasOne(ws => ws.Workstation)
            .WithMany(w => w.WorkstationStatuses)
            .HasForeignKey(ws => ws.WorkstationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Support → LocalizationHistory (1:N)
        modelBuilder.Entity<LocalizationHistoryModel>()
            .HasOne(lh => lh.Support)
            .WithMany(s => s.LocalizationHistories)
            .HasForeignKey(lh => lh.SupportId)
            .OnDelete(DeleteBehavior.Restrict);

        // Workstation → LocalizationHistory (1:N)
        modelBuilder.Entity<LocalizationHistoryModel>()
            .HasOne(lh => lh.Workstation)
            .WithMany(w => w.LocalizationHistories)
            .HasForeignKey(lh => lh.WorkstationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Support → SupportedProduct (1:N)
        modelBuilder.Entity<SupportedProductModel>()
            .HasOne(sp => sp.Support)
            .WithMany(s => s.SupportedProducts)
            .HasForeignKey(sp => sp.SupportId)
            .OnDelete(DeleteBehavior.Restrict);

        // Product → SupportedProduct (1:N, opcional)
        modelBuilder.Entity<SupportedProductModel>()
            .HasOne(sp => sp.Product)
            .WithMany(p => p.SupportedProducts)
            .HasForeignKey(sp => sp.ProductId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // CarModel → PhaseSequence (1:N)
        modelBuilder.Entity<PhaseSequenceModel>()
            .HasOne(ps => ps.CarModel)
            .WithMany(m => m.PhaseSequences)
            .HasForeignKey(ps => ps.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        // ManufacturingPhase → PhaseSequence (1:N)
        modelBuilder.Entity<PhaseSequenceModel>()
            .HasOne(ps => ps.ManufacturingPhase)
            .WithMany(mp => mp.PhaseSequences)
            .HasForeignKey(ps => ps.ManufacturingPhaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // CarModel → ModelMaterial (1:N)
        modelBuilder.Entity<ModelMaterialModel>()
            .HasOne(mm => mm.CarModel)
            .WithMany(m => m.ModelMaterials)
            .HasForeignKey(mm => mm.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        // Material → ModelMaterial (1:N)
        modelBuilder.Entity<ModelMaterialModel>()
            .HasOne(mm => mm.Material)
            .WithMany(m => m.ModelMaterials)
            .HasForeignKey(mm => mm.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        // ManufacturingPhase → ModelMaterial (1:N)
        modelBuilder.Entity<ModelMaterialModel>()
            .HasOne(mm => mm.ManufacturingPhase)
            .WithMany(mp => mp.ModelMaterials)
            .HasForeignKey(mm => mm.ManufacturingPhaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // CarModel → Config (1:N)
        modelBuilder.Entity<ConfigModel>()
            .HasOne(c => c.CarModel)
            .WithMany(m => m.Configs)
            .HasForeignKey(c => c.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        // ClientOrder → ManufacturingOrder (1:N)
        modelBuilder.Entity<ManufacturingOrderModel>()
            .HasOne(mo => mo.ClientOrder)
            .WithMany(co => co.ManufacturingOrders)
            .HasForeignKey(mo => mo.ClientOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // ManufacturingOrder → Product (1:N)
        modelBuilder.Entity<ProductModel>()
            .HasOne(p => p.ManufacturingOrder)
            .WithMany(mo => mo.Products)
            .HasForeignKey(p => p.ManufacturingOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // CarModel → Product (1:N)
        modelBuilder.Entity<ProductModel>()
            .HasOne(p => p.CarModel)
            .WithMany(m => m.Products)
            .HasForeignKey(p => p.ModelId)
            .OnDelete(DeleteBehavior.Restrict);

        // Product → ProductPhase (1:N)
        modelBuilder.Entity<ProductPhaseModel>()
            .HasOne(pp => pp.Product)
            .WithMany(p => p.ProductPhases)
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ManufacturingPhase → ProductPhase (1:N)
        modelBuilder.Entity<ProductPhaseModel>()
            .HasOne(pp => pp.ManufacturingPhase)
            .WithMany(mp => mp.ProductPhases)
            .HasForeignKey(pp => pp.ManufacturingPhaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Workstation → ProductPhase (1:N)
        modelBuilder.Entity<ProductPhaseModel>()
            .HasOne(pp => pp.Workstation)
            .WithMany(w => w.ProductPhases)
            .HasForeignKey(pp => pp.WorkstationId)
            .OnDelete(DeleteBehavior.Restrict);

        // QualityCheck → ProductPhase (1:N, opcional)
        modelBuilder.Entity<ProductPhaseModel>()
            .HasOne(pp => pp.QualityCheck)
            .WithMany(qc => qc.ProductPhases)
            .HasForeignKey(pp => pp.QualityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Product → QualityCheck (1:N)
        modelBuilder.Entity<QualityCheckModel>()
            .HasOne(qc => qc.Product)
            .WithMany(p => p.QualityChecks)
            .HasForeignKey(qc => qc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ManufacturingPhase → QualityCheck (1:N)
        modelBuilder.Entity<QualityCheckModel>()
            .HasOne(qc => qc.ManufacturingPhase)
            .WithMany(mp => mp.QualityChecks)
            .HasForeignKey(qc => qc.ManufacturingPhaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Config → ConfigOption (1:N)
        modelBuilder.Entity<ConfigOptionModel>()
            .HasOne(co => co.Config)
            .WithMany(c => c.ConfigOptions)
            .HasForeignKey(co => co.ConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product → ProductConfig (1:N)
        modelBuilder.Entity<ProductConfigModel>()
            .HasOne(pc => pc.Product)
            .WithMany(p => p.ProductConfigs)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ConfigOption → ProductConfig (1:N)
        modelBuilder.Entity<ProductConfigModel>()
            .HasOne(pc => pc.ConfigOption)
            .WithMany() 
            .HasForeignKey(pc => pc.ConfigOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Product → Alert (1:N)
        modelBuilder.Entity<AlertModel>()
            .HasOne(a => a.Product)
            .WithMany(p => p.Alerts)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProductPhase → Alert (1:N)
        modelBuilder.Entity<AlertModel>()
            .HasOne(a => a.ProductPhase)
            .WithMany(pp => pp.Alerts)
            .HasForeignKey(a => a.ProductPhaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}