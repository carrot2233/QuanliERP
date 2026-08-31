using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Material> Materials => Set<Material>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<Employee> Employees => Set<Employee>();

        public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
        public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
        public DbSet<Delivery> Deliveries => Set<Delivery>();
        public DbSet<DeliveryItem> DeliveryItems => Set<DeliveryItem>();

        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
        public DbSet<PurchaseReceipt> PurchaseReceipts => Set<PurchaseReceipt>();
        public DbSet<PurchaseReceiptItem> PurchaseReceiptItems => Set<PurchaseReceiptItem>();

        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<InventoryLedger> InventoryLedgers => Set<InventoryLedger>();

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        public DbSet<ProductionPlan> ProductionPlans => Set<ProductionPlan>();
        public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
        public DbSet<ProductionDailyReport> ProductionDailyReports => Set<ProductionDailyReport>();
        public DbSet<DailyReportProcess> DailyReportProcesses => Set<DailyReportProcess>();

        public DbSet<QualityInspection> QualityInspections => Set<QualityInspection>();
        public DbSet<MeasuringTool> MeasuringTools => Set<MeasuringTool>();
        public DbSet<ToolApply> ToolApplies => Set<ToolApply>();
        public DbSet<ToolScrap> ToolScraps => Set<ToolScrap>();
        public DbSet<ToolCalibration> ToolCalibrations => Set<ToolCalibration>();

        public DbSet<Equipment> Equipments => Set<Equipment>();
        public DbSet<EquipmentMaintenance> EquipmentMaintenances => Set<EquipmentMaintenance>();

        public DbSet<Shift> Shifts => Set<Shift>();
        public DbSet<WorkSchedule> WorkSchedules => Set<WorkSchedule>();

        public DbSet<Mold> Molds => Set<Mold>();
        public DbSet<MoldPlan> MoldPlans => Set<MoldPlan>();
        public DbSet<MoldPlanStage> MoldPlanStages => Set<MoldPlanStage>();

        // 协同办公
        public DbSet<Notice> Notices => Set<Notice>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<FlowDesign> FlowDesigns => Set<FlowDesign>();
        public DbSet<FlowNode> FlowNodes => Set<FlowNode>();
        public DbSet<FlowInstance> FlowInstances => Set<FlowInstance>();
        public DbSet<FlowTask> FlowTasks => Set<FlowTask>();
        public DbSet<FileRecord> FileRecords => Set<FileRecord>();

        // 人力资源
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<Payroll> Payrolls => Set<Payroll>();
        public DbSet<Training> Trainings => Set<Training>();
        public DbSet<TrainingParticipant> TrainingParticipants => Set<TrainingParticipant>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<Customer>().HasIndex(c => c.Code).IsUnique();
            modelBuilder.Entity<Supplier>().HasIndex(s => s.Code).IsUnique();
            modelBuilder.Entity<Material>().HasIndex(m => m.Code).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.Code).IsUnique();
            modelBuilder.Entity<Warehouse>().HasIndex(w => w.Code).IsUnique();
            modelBuilder.Entity<Employee>().HasIndex(e => e.Code).IsUnique();
            modelBuilder.Entity<Equipment>().HasIndex(e => e.Code).IsUnique();

            modelBuilder.Entity<SalesOrder>()
                .HasOne(o => o.Customer).WithMany()
                .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrderItem>()
                .HasOne(i => i.SalesOrder).WithMany(o => o.Items)
                .HasForeignKey(i => i.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SalesOrderItem>()
                .HasOne(i => i.Product).WithMany()
                .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.SalesOrder).WithMany()
                .HasForeignKey(d => d.SalesOrderId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Customer).WithMany()
                .HasForeignKey(d => d.CustomerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Warehouse).WithMany()
                .HasForeignKey(d => d.WarehouseId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeliveryItem>()
                .HasOne(i => i.Delivery).WithMany(d => d.Items)
                .HasForeignKey(i => i.DeliveryId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DeliveryItem>()
                .HasOne(i => i.Product).WithMany()
                .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(o => o.Supplier).WithMany()
                .HasForeignKey(o => o.SupplierId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(i => i.PurchaseOrder).WithMany(o => o.Items)
                .HasForeignKey(i => i.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(i => i.Material).WithMany()
                .HasForeignKey(i => i.MaterialId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseReceipt>()
                .HasOne(r => r.PurchaseOrder).WithMany()
                .HasForeignKey(r => r.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PurchaseReceipt>()
                .HasOne(r => r.Supplier).WithMany()
                .HasForeignKey(r => r.SupplierId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PurchaseReceipt>()
                .HasOne(r => r.Warehouse).WithMany()
                .HasForeignKey(r => r.WarehouseId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseReceiptItem>()
                .HasOne(i => i.PurchaseReceipt).WithMany(r => r.Items)
                .HasForeignKey(i => i.PurchaseReceiptId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PurchaseReceiptItem>()
                .HasOne(i => i.Material).WithMany()
                .HasForeignKey(i => i.MaterialId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Warehouse).WithMany()
                .HasForeignKey(i => i.WarehouseId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role).WithMany()
                .HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionCode }).IsUnique();

            modelBuilder.Entity<ProductionPlan>()
                .HasOne(p => p.Customer).WithMany()
                .HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductionPlan>()
                .HasOne(p => p.Product).WithMany()
                .HasForeignKey(p => p.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductionPlan>()
                .HasOne(p => p.Material).WithMany()
                .HasForeignKey(p => p.MaterialId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductionDailyReport>()
                .HasMany(r => r.Processes).WithOne(p => p.ProductionDailyReport)
                .HasForeignKey(p => p.ProductionDailyReportId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QualityInspection>()
                .HasOne(q => q.Product).WithMany()
                .HasForeignKey(q => q.ProductId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mold>()
                .HasOne(m => m.Customer).WithMany()
                .HasForeignKey(m => m.CustomerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Mold>()
                .HasOne(m => m.Product).WithMany()
                .HasForeignKey(m => m.ProductId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MoldPlan>()
                .HasOne(p => p.Customer).WithMany()
                .HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MoldPlanStage>()
                .HasOne(s => s.MoldPlan).WithMany(p => p.Stages)
                .HasForeignKey(s => s.MoldPlanId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkSchedule>()
                .HasOne(s => s.Employee).WithMany()
                .HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WorkSchedule>()
                .HasOne(s => s.Shift).WithMany()
                .HasForeignKey(s => s.ShiftId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EquipmentMaintenance>()
                .HasOne(m => m.Equipment).WithMany()
                .HasForeignKey(m => m.EquipmentId).OnDelete(DeleteBehavior.Cascade);

            // 协同办公
            modelBuilder.Entity<FlowNode>()
                .HasOne(n => n.FlowDesign).WithMany(d => d.Nodes)
                .HasForeignKey(n => n.FlowDesignId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FlowTask>()
                .HasOne(t => t.FlowInstance).WithMany(i => i.Tasks)
                .HasForeignKey(t => t.FlowInstanceId).OnDelete(DeleteBehavior.Cascade);

            // 人力资源
            modelBuilder.Entity<TrainingParticipant>()
                .HasOne(p => p.Training).WithMany(t => t.Participants)
                .HasForeignKey(p => p.TrainingId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
