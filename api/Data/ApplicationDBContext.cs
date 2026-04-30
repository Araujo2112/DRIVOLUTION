using ApiTexPact.Models;
using ApiTexPact.Services;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ContainerModel> Containers { get; set; }
    public DbSet<ItemInContainerModel> ItemInContainer { get; set; }
    public DbSet<ContainerLocalizationModel> ContainerLocalization { get; set; }
    public DbSet<PlantFloorSectionModel> PlantFloorSection { get; set; }
    public DbSet<ItemLocalizationModel> ItemLocalization { get; set; }
    public DbSet<ItemOfRawMaterialModel> ItemOfRawMaterial { get; set; }
    public DbSet<LotOfRawMaterialModel> LotOfRawMaterial { get; set; }
    public DbSet<RawMaterialModel> RawMaterial { get; set; }
    public DbSet<EmployeeModel> Employees { get; set; }
    public DbSet<CheckpointModel> Checkpoints { get; set; }

    public DbSet<ProductionLineModel> ProductionLines { get; set; }


    // Second Part API:

    public DbSet<ManufacturingOrderHistoryModel> ManufacturingOrderHistories { get; set; }
    public DbSet<ClientModel> Clients { get; set; }
    public DbSet<ManufacturingOrderModel> ManufacturingOrders { get; set; }
    public DbSet<ManufacturingOrderPhaseModel> ManufacturingOrderPhases { get; set; }
    public DbSet<ManufacturingProcessModel> ManufacturingProcesses { get; set; }
    public DbSet<ManufacturingPhaseModel> ManufacturingPhases { get; set; }
    public DbSet<ManufacturingProcessPhaseModel> ManufacturingProcessPhases { get; set; }
    public DbSet<ProductModel> Products { get; set; }
    public DbSet<ProductLotModel> ProductLots { get; set; }

    // AI

    public DbSet<PredictionModel> Prediction { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<ContainerLocalizationModel>()
            .HasKey(cl => cl.Id);

        modelBuilder.Entity<ContainerLocalizationModel>()
            .HasIndex(cl => new { cl.ContainerId, cl.SectionId })
            .IsUnique(false);
        
        modelBuilder.Entity<ItemOfRawMaterialModel>()
            .HasMany(i => i.ItemLocalizations)
            .WithOne(il => il.ItemOfRawMaterial)
            .HasForeignKey(il => il.ItemRawId)
            .OnDelete(DeleteBehavior.Cascade);

        
        modelBuilder.Entity<ItemLocalizationModel>()
            .HasKey(i => i.ItemLocalizationId);

        modelBuilder.Entity<ItemLocalizationModel>()
            .HasOne(i => i.ContainerLocalization)
            .WithMany(c => c.ItemLocalizations)
            .HasForeignKey(i => i.ContainerLocalizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento entre LotOfRawMaterial e RawMaterials
        modelBuilder.Entity<LotOfRawMaterialModel>()
            .HasOne(l => l.RawMaterials)
            .WithMany(r => r.LotOfRawMaterials)
            .HasForeignKey(l => l.RawMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento entre ItemInContainer e Container
        modelBuilder.Entity<ItemInContainerModel>()
            .HasOne(i => i.Container)
            .WithMany(c => c.IteminContainers)
            .HasForeignKey(i => i.ContainerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento entre ContainerLocalization e Container
        modelBuilder.Entity<ContainerLocalizationModel>()
            .HasOne(c => c.Container)
            .WithMany(l => l.LocalizationHistories)
            .HasForeignKey(l => l.ContainerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento entre ContainerLocalization e PlantFloorSection
        modelBuilder.Entity<ContainerLocalizationModel>()
            .HasOne(p => p.PlantFloorSection)
            .WithMany(pfs => pfs.LocalizationHistories)
            .HasForeignKey(l => l.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento entre ItemInContainer e ItemOfRawMaterial
        modelBuilder.Entity<ItemInContainerModel>()
            .HasMany(i => i.ItemsOfRawMaterial)
            .WithOne(i => i.ItemInContainer)
            .HasForeignKey(i => i.ItemInContainerId);

        // Relacionamento entre ItemOfRawMaterial e LotOfRawMaterial
        modelBuilder.Entity<ItemOfRawMaterialModel>()
            .HasOne(i => i.LotOfRawMaterial)
            .WithMany(l => l.ItemOfRawMaterials)
            .HasForeignKey(i => i.LotOfRawMaterialId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relacionamento entre Checkpoint e PlantFloorSection
        modelBuilder.Entity<CheckpointModel>()
            .HasOne(c => c.PlantFloorSection)
            .WithMany(p => p.Checkpoints)
            .HasForeignKey(c => c.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ManufacturingPhaseModel>()
            .HasOne(a => a.PlantFloorSection)
            .WithMany()
            .HasForeignKey(a => a.PlantFloorSectionId);

        // Manufacturing Order History
        modelBuilder.Entity<ManufacturingOrderHistoryModel>()
            .HasKey(mh => new { mh.ManufacturingOrderId, mh.PlantFloorSectionId });

        modelBuilder.Entity<ManufacturingOrderHistoryModel>()
            .HasOne(mh => mh.ManufacturingOrder)
            .WithMany(m => m.ManufacturingOrderHistory)
            .HasForeignKey(mh => mh.ManufacturingOrderId);

        modelBuilder.Entity<ManufacturingOrderHistoryModel>()
            .HasOne(mh => mh.PlantFloorSection)
            .WithMany(pf => pf.OrderHistory)
            .HasForeignKey(mh => mh.PlantFloorSectionId);

        // Manufacturing Order Phases
        modelBuilder.Entity<ManufacturingOrderPhaseModel>()
            .HasOne(mop => mop.ManufacturingOrder)
            .WithMany(mo => mo.ManufacturingOrderPhases)
            .HasForeignKey(mop => mop.ManufacturingOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ManufacturingOrderPhaseModel>()
            .HasOne(mop => mop.ManufacturingPhase)
            .WithMany(mp => mp.ManufacturingOrderPhases)
            .HasForeignKey(mop => mop.ManufacturingPhaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemOfRawMaterialModel>()
            .HasOne(i => i.ManufacturingOrderPhase)
            .WithMany(mop => mop.ItemOfRawMaterials)
            .HasForeignKey(i => i.ManufacturingOrderPhaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Manufacturing Process
        modelBuilder.Entity<ManufacturingProcessModel>()
            .HasOne(mp => mp.Product)
            .WithMany(p => p.ManufacturingProcesses)
            .HasForeignKey(mp => mp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ManufacturingProcessPhaseModel>()
            .HasOne(mpp => mpp.ManufacturingProcess)
            .WithMany(mp => mp.ManufacturingProcessPhases)
            .HasForeignKey(mpp => mpp.ManufacturingProcessId)
            .OnDelete(DeleteBehavior.Cascade);

        // Manufacturing Phase
        modelBuilder.Entity<ManufacturingPhaseModel>()
            .HasOne(mp => mp.PlantFloorSection)
            .WithOne(pfs => pfs.ManufacturingPhase)
            .HasForeignKey<ManufacturingPhaseModel>(mp => mp.PlantFloorSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ManufacturingProcessPhase
        modelBuilder.Entity<ManufacturingProcessPhaseModel>()
            .HasKey(mpp => new { mpp.ManufacturingPhaseId, mpp.ManufacturingProcessId });

        modelBuilder.Entity<ManufacturingProcessPhaseModel>()
            .HasOne(mpp => mpp.ManufacturingPhase)
            .WithMany(mp => mp.ManufacturingProcessPhases)
            .HasForeignKey(mpp => mpp.ManufacturingPhaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ManufacturingProcessPhaseModel>()
            .HasOne(mpp => mpp.ManufacturingProcess)
            .WithMany(mp => mp.ManufacturingProcessPhases)
            .HasForeignKey(mpp => mpp.ManufacturingProcessId)
            .OnDelete(DeleteBehavior.Cascade);

        // CONSOLIDADO: Manufacturing Order relationships (removidas duplicações)
        modelBuilder.Entity<ManufacturingOrderModel>()
            .HasOne(mo => mo.Client)
            .WithMany(c => c.ManufacturingOrders)
            .HasForeignKey(mo => mo.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ManufacturingOrderModel>()
            .HasOne(mo => mo.ManufacturingProcess)
            .WithMany(mp => mp.ManufacturingOrders)
            .HasForeignKey(mo => mo.ManufacturingProcessId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ManufacturingOrderModel>()
            .HasOne(mo => mo.ProductLot)
            .WithMany(pl => pl.ManufacturingOrders)
            .HasForeignKey(mo => mo.ProductLotId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ManufacturingOrderModel>()
            .HasMany(mo => mo.ItemsOfRawMaterial)
            .WithOne(iorm => iorm.ManufacturingOrder)
            .HasForeignKey(iorm => iorm.ManufacturingOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product Lot
        modelBuilder.Entity<ProductLotModel>()
            .HasOne(pl => pl.Product)
            .WithMany(p => p.ProductLots)
            .HasForeignKey(pl => pl.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product
        modelBuilder.Entity<ProductModel>()
            .HasMany(p => p.ManufacturingProcesses)
            .WithOne(mp => mp.Product)
            .HasForeignKey(mp => mp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductModel>()
            .HasMany(p => p.ProductLots)
            .WithOne(pl => pl.Product)
            .HasForeignKey(pl => pl.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Employee relationships
        modelBuilder.Entity<EmployeeModel>()
            .HasOne(e => e.ManufacturingPhase)
            .WithMany()
            .HasForeignKey(e => e.ManufacturingPhaseId)
            .IsRequired(false);

        modelBuilder.Entity<SectionAdminModel>()
            .HasOne(psa => psa.Employee)
            .WithOne()
            .HasForeignKey<SectionAdminModel>(psa => psa.EmployeeId);

        modelBuilder.Entity<SectionAdminModel>()
            .HasOne(psa => psa.PlantFloorSection)
            .WithOne()
            .HasForeignKey<SectionAdminModel>(psa => psa.PlantFloorSectionId);

        modelBuilder.Entity<SectionAdminModel>()
            .HasIndex(psa => psa.EmployeeId)
            .IsUnique();

        modelBuilder.Entity<SectionAdminModel>()
            .HasIndex(psa => psa.PlantFloorSectionId)
            .IsUnique();

        // Seed data
        // AI

        modelBuilder.Entity<PredictionModel>()
            .HasIndex(p => p.Id)
            .IsUnique();

        modelBuilder.Entity<PredictionModel>()
            .Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<EmployeeModel>()
            .HasData(new EmployeeModel
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Username = "admin",
                Password = EmployeeService.HashPassword("admin")
            });
    }
}
