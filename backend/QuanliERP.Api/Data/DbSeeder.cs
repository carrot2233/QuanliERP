using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            if (db.Users.Any()) return;

            // ---------- 用户 ----------
            db.Users.AddRange(
                new User { Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), DisplayName = "系统管理员", Role = "admin", IsActive = true },
                new User { Username = "pro", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), DisplayName = "生产主管", Role = "production", IsActive = true },
                new User { Username = "wh", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), DisplayName = "仓库管理员", Role = "warehouse", IsActive = true },
                new User { Username = "qa", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), DisplayName = "质量检验员", Role = "quality", IsActive = true },
                new User { Username = "sale", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), DisplayName = "销售专员", Role = "sales", IsActive = true });
            db.SaveChanges();

            // ---------- 客户 ----------
            var c1 = new Customer { Code = "C001", Name = "海马汽车有限公司", Contact = "李经理", Phone = "13803910001", Address = "河南郑州", Remark = "主要冲压件客户" };
            var c2 = new Customer { Code = "C002", Name = "比亚迪零部件事业部", Contact = "王工", Phone = "13803910002", Address = "广东深圳", Remark = "汽车冲压件" };
            var c3 = new Customer { Code = "C003", Name = "中航光电", Contact = "张工", Phone = "13803910003", Address = "河南鹤壁", Remark = "连接器冲压件" };
            db.Customers.AddRange(c1, c2, c3);

            // ---------- 供应商 ----------
            var s1 = new Supplier { Code = "S001", Name = "安阳钢铁集团", Contact = "赵总", Phone = "13803910011", Address = "河南安阳", PaymentTerms = "月结30天", Remark = "冷轧板供应" };
            var s2 = new Supplier { Code = "S002", Name = "宝武集团武汉分公司", Contact = "钱总", Phone = "13803910012", Address = "湖北武汉", PaymentTerms = "月结45天", Remark = "高强度钢供应" };
            var s3 = new Supplier { Code = "S003", Name = "鹤壁五金工具商行", Contact = "孙经理", Phone = "13803910013", Address = "河南鹤壁", PaymentTerms = "现款现货", Remark = "刀具量具" };
            db.Suppliers.AddRange(s1, s2, s3);

            // ---------- 仓库 ----------
            var wRaw = new Warehouse { Code = "W001", Name = "原材料库", Location = "冲压车间东侧", Manager = "仓库管理员" };
            var wFin = new Warehouse { Code = "W002", Name = "成品库", Location = "车间西侧", Manager = "仓库管理员" };
            db.Warehouses.AddRange(wRaw, wFin);

            // ---------- 原材料（板材，取自原材料库存表） ----------
            var m1 = new Material { Code = "M01", Name = "顶盖前横梁板材", Specification = "1.0*1700*400-HC340/590DP", MaterialType = "HC340/590DP", Unit = "张", Category = "板材", UnitPrice = 86, MinStock = 100 };
            var m2 = new Material { Code = "M06", Name = "行车记录仪后纵梁板材", Specification = "0.8*880*480-DC01", MaterialType = "DC01", Unit = "张", Category = "板材", UnitPrice = 58, MinStock = 100 };
            var m3 = new Material { Code = "M45", Name = "阅读灯安装支架板材", Specification = "0.8*480*280-DC51D+Z", MaterialType = "DC51D+Z", Unit = "张", Category = "板材", UnitPrice = 42, MinStock = 100 };
            var m4 = new Material { Code = "M51", Name = "行车记录仪下安装支架板材", Specification = "0.8*335*790-DC01", MaterialType = "DC01", Unit = "张", Category = "板材", UnitPrice = 46, MinStock = 80 };
            var m5 = new Material { Code = "M55", Name = "行车记录仪上安装支架板材", Specification = "0.8*205*725-DC01", MaterialType = "DC01", Unit = "张", Category = "板材", UnitPrice = 31, MinStock = 80 };
            var m6 = new Material { Code = "M57", Name = "电视机支架板材", Specification = "1.5*295*835-HC340/590DP", MaterialType = "HC340/590DP", Unit = "张", Category = "板材", UnitPrice = 93, MinStock = 80 };
            var m7 = new Material { Code = "M99", Name = "冲压模具钢", Specification = "Cr12MoV", MaterialType = "模具钢", Unit = "kg", Category = "刀具材料", UnitPrice = 25, MinStock = 500 };
            var m8 = new Material { Code = "M98", Name = "硬质合金刀具", Specification = "YG6", MaterialType = "硬质合金", Unit = "把", Category = "刀具材料", UnitPrice = 120, MinStock = 20 };
            db.Materials.AddRange(m1, m2, m3, m4, m5, m6, m7, m8);

            // ---------- 产品（冲压件） ----------
            var p1 = new Product { Code = "5701-99566", Name = "顶盖前横梁", Specification = "1.0*1700*400", Material = "HC340/590DP", CustPartNo = "5701-99566", MoldNo = "M01", Unit = "件", OneOutputs = 1, ProcessRoute = "落料-拉延-修边-冲孔侧冲", StandardHours = 0.5m, SalePrice = 12.5m };
            var p2 = new Product { Code = "5701-99570", Name = "行车记录仪后纵梁", Specification = "0.8*880*480", Material = "DC01", CustPartNo = "5701-99570", MoldNo = "M06", Unit = "件", OneOutputs = 1, ProcessRoute = "落料-拉延-修边-冲孔侧冲", StandardHours = 0.4m, SalePrice = 8.6m };
            var p3 = new Product { Code = "5701-99573", Name = "阅读灯安装支架", Specification = "0.8*480*280", Material = "DC51D+Z", CustPartNo = "5701-99573", MoldNo = "M45", Unit = "件", OneOutputs = 1, ProcessRoute = "落料-拉延-修边", StandardHours = 0.3m, SalePrice = 5.2m };
            var p4 = new Product { Code = "5701-A1007", Name = "行车记录仪下安装支架", Specification = "0.8*335*790", Material = "DC01", CustPartNo = "5701-A1007", MoldNo = "M51", Unit = "件", OneOutputs = 5, ProcessRoute = "落料-拉延-冲孔侧冲", StandardHours = 0.35m, SalePrice = 6.8m };
            var p5 = new Product { Code = "5701-A1008", Name = "行车记录仪上安装支架", Specification = "0.8*205*725", Material = "DC01", CustPartNo = "5701-A1008", MoldNo = "M55", Unit = "件", OneOutputs = 10, ProcessRoute = "落料-拉延-冲孔侧冲", StandardHours = 0.3m, SalePrice = 4.5m };
            var p6 = new Product { Code = "5701-99575", Name = "电视机支架", Specification = "1.5*295*835", Material = "HC340/590DP", CustPartNo = "5701-99575", MoldNo = "M57", Unit = "件", OneOutputs = 6, ProcessRoute = "落料-拉延-修边-冲孔侧冲", StandardHours = 0.45m, SalePrice = 9.8m };
            db.Products.AddRange(p1, p2, p3, p4, p5, p6);
            db.SaveChanges();

            // ---------- 员工 ----------
            var e1 = new Employee { Code = "E001", Name = "张伟", Gender = "男", Dept = "冲压车间", Position = "冲压工", Phone = "13800000001", Status = "在职" };
            var e2 = new Employee { Code = "E002", Name = "李强", Gender = "男", Dept = "冲压车间", Position = "冲压工", Phone = "13800000002", Status = "在职" };
            var e3 = new Employee { Code = "E003", Name = "王芳", Gender = "女", Dept = "冲压车间", Position = "操作工", Phone = "13800000003", Status = "在职" };
            var e4 = new Employee { Code = "E004", Name = "刘洋", Gender = "男", Dept = "模具车间", Position = "模具钳工", Phone = "13800000004", Status = "在职" };
            var e5 = new Employee { Code = "E005", Name = "陈静", Gender = "女", Dept = "质检部", Position = "检验员", Phone = "13800000005", Status = "在职" };
            db.Employees.AddRange(e1, e2, e3, e4, e5);

            // ---------- 班次 ----------
            var sh1 = new Shift { Name = "早班", StartTime = "08:00", EndTime = "16:00" };
            var sh2 = new Shift { Name = "中班", StartTime = "16:00", EndTime = "00:00" };
            var sh3 = new Shift { Name = "夜班", StartTime = "00:00", EndTime = "08:00" };
            db.Shifts.AddRange(sh1, sh2, sh3);

            // ---------- 设备 ----------
            var eq1 = new Equipment { Code = "EQ001", Name = "闭式双点冲床", Model = "J31-250", EquipType = "冲床", Tonnage = 250, Workshop = "冲压车间", Status = "运行", Manufacturer = "济南二机床", PurchaseDate = new DateTime(2021, 3, 12), MaintenanceCycle = "月度保养", LastMaintainDate = new DateTime(2026, 7, 15), NextMaintainDate = new DateTime(2026, 8, 15) };
            var eq2 = new Equipment { Code = "EQ002", Name = "四柱油压机", Model = "YJ32-315", EquipType = "油压机", Tonnage = 315, Workshop = "冲压车间", Status = "运行", Manufacturer = "合肥合锻", PurchaseDate = new DateTime(2020, 5, 20), MaintenanceCycle = "月度保养", LastMaintainDate = new DateTime(2026, 7, 10), NextMaintainDate = new DateTime(2026, 8, 10) };
            var eq3 = new Equipment { Code = "EQ003", Name = "液压剪板机", Model = "QC12Y-16*2500", EquipType = "剪板机", Tonnage = 16, Workshop = "下料车间", Status = "运行", Manufacturer = "上海冲剪", PurchaseDate = new DateTime(2019, 8, 1), MaintenanceCycle = "季度保养", LastMaintainDate = new DateTime(2026, 5, 20), NextMaintainDate = new DateTime(2026, 8, 20) };
            var eq4 = new Equipment { Code = "EQ004", Name = "线切割机床", Model = "DK7725", EquipType = "线切割", Tonnage = 0, Workshop = "模具车间", Status = "运行", Manufacturer = "苏州三光", PurchaseDate = new DateTime(2021, 11, 5), MaintenanceCycle = "季度保养", LastMaintainDate = new DateTime(2026, 6, 1), NextMaintainDate = new DateTime(2026, 9, 1) };
            var eq5 = new Equipment { Code = "EQ005", Name = "卧式车床", Model = "CA6140", EquipType = "车床", Tonnage = 0, Workshop = "模具车间", Status = "维修", Manufacturer = "沈阳机床", PurchaseDate = new DateTime(2018, 4, 15), MaintenanceCycle = "季度保养", LastMaintainDate = new DateTime(2026, 6, 15), NextMaintainDate = new DateTime(2026, 9, 15) };
            db.Equipments.AddRange(eq1, eq2, eq3, eq4, eq5);
            db.SaveChanges();

            // ---------- 生产计划（制号） ----------
            db.ProductionPlans.AddRange(
                new ProductionPlan { PlanNo = "M01", CustomerId = c1.Id, ProjectName = "顶盖前横梁项目", ProductId = p1.Id, MaterialId = m1.Id, OneOutputs = 1, PlanQty = 10000, PlannedStart = new DateTime(2026, 8, 1), PlannedEnd = new DateTime(2026, 8, 31), Status = "进行中", Remark = "重点订单" },
                new ProductionPlan { PlanNo = "M06", CustomerId = c1.Id, ProjectName = "行车记录仪后纵梁项目", ProductId = p2.Id, MaterialId = m2.Id, OneOutputs = 1, PlanQty = 8000, PlannedStart = new DateTime(2026, 8, 1), PlannedEnd = new DateTime(2026, 8, 28), Status = "进行中" },
                new ProductionPlan { PlanNo = "M45", CustomerId = c2.Id, ProjectName = "阅读灯安装支架项目", ProductId = p3.Id, MaterialId = m3.Id, OneOutputs = 1, PlanQty = 6000, PlannedStart = new DateTime(2026, 7, 1), PlannedEnd = new DateTime(2026, 8, 20), Status = "进行中" },
                new ProductionPlan { PlanNo = "M51", CustomerId = c2.Id, ProjectName = "行车记录仪下安装支架项目", ProductId = p4.Id, MaterialId = m4.Id, OneOutputs = 5, PlanQty = 3000, PlannedStart = new DateTime(2026, 7, 15), PlannedEnd = new DateTime(2026, 8, 15), Status = "已完成", ActualEnd = new DateTime(2026, 8, 5) },
                new ProductionPlan { PlanNo = "M55", CustomerId = c3.Id, ProjectName = "行车记录仪上安装支架项目", ProductId = p5.Id, MaterialId = m5.Id, OneOutputs = 10, PlanQty = 5000, PlannedStart = new DateTime(2026, 8, 5), PlannedEnd = new DateTime(2026, 8, 25), Status = "未开始" },
                new ProductionPlan { PlanNo = "M57", CustomerId = c1.Id, ProjectName = "电视机支架项目", ProductId = p6.Id, MaterialId = m6.Id, OneOutputs = 6, PlanQty = 4000, PlannedStart = new DateTime(2026, 7, 1), PlannedEnd = new DateTime(2026, 7, 31), Status = "已完成", ActualEnd = new DateTime(2026, 7, 28) });
            db.SaveChanges();

            // ---------- 库存（原材料，取自材料总表结存） ----------
            db.Inventories.AddRange(
                new Inventory { WarehouseId = wRaw.Id, ItemType = "材料", ItemId = m1.Id, Code = m1.Code, Name = m1.Name, Specification = m1.Specification, Unit = m1.Unit, Qty = 572, SafeStock = m1.MinStock },
                new Inventory { WarehouseId = wRaw.Id, ItemType = "材料", ItemId = m2.Id, Code = m2.Code, Name = m2.Name, Specification = m2.Specification, Unit = m2.Unit, Qty = 0, SafeStock = m2.MinStock },
                new Inventory { WarehouseId = wRaw.Id, ItemType = "材料", ItemId = m3.Id, Code = m3.Code, Name = m3.Name, Specification = m3.Specification, Unit = m3.Unit, Qty = 800, SafeStock = m3.MinStock },
                new Inventory { WarehouseId = wRaw.Id, ItemType = "材料", ItemId = m4.Id, Code = m4.Code, Name = m4.Name, Specification = m4.Specification, Unit = m4.Unit, Qty = 100, SafeStock = m4.MinStock },
                new Inventory { WarehouseId = wRaw.Id, ItemType = "材料", ItemId = m5.Id, Code = m5.Code, Name = m5.Name, Specification = m5.Specification, Unit = m5.Unit, Qty = 0, SafeStock = m5.MinStock },
                new Inventory { WarehouseId = wRaw.Id, ItemType = "材料", ItemId = m6.Id, Code = m6.Code, Name = m6.Name, Specification = m6.Specification, Unit = m6.Unit, Qty = 0, SafeStock = m6.MinStock },
                new Inventory { WarehouseId = wRaw.Id, ItemType = "材料", ItemId = m7.Id, Code = m7.Code, Name = m7.Name, Specification = m7.Specification, Unit = m7.Unit, Qty = 320, SafeStock = m7.MinStock },
                new Inventory { WarehouseId = wRaw.Id, ItemType = "材料", ItemId = m8.Id, Code = m8.Code, Name = m8.Name, Specification = m8.Specification, Unit = m8.Unit, Qty = 12, SafeStock = m8.MinStock },
                new Inventory { WarehouseId = wFin.Id, ItemType = "产品", ItemId = p1.Id, Code = p1.Code, Name = p1.Name, Specification = p1.Specification, Unit = p1.Unit, Qty = 860, SafeStock = 500 },
                new Inventory { WarehouseId = wFin.Id, ItemType = "产品", ItemId = p2.Id, Code = p2.Code, Name = p2.Name, Specification = p2.Specification, Unit = p2.Unit, Qty = 1200, SafeStock = 500 },
                new Inventory { WarehouseId = wFin.Id, ItemType = "产品", ItemId = p3.Id, Code = p3.Code, Name = p3.Name, Specification = p3.Specification, Unit = p3.Unit, Qty = 640, SafeStock = 400 },
                new Inventory { WarehouseId = wFin.Id, ItemType = "产品", ItemId = p4.Id, Code = p4.Code, Name = p4.Name, Specification = p4.Specification, Unit = p4.Unit, Qty = 320, SafeStock = 400 },
                new Inventory { WarehouseId = wFin.Id, ItemType = "产品", ItemId = p5.Id, Code = p5.Code, Name = p5.Name, Specification = p5.Specification, Unit = p5.Unit, Qty = 0, SafeStock = 400 },
                new Inventory { WarehouseId = wFin.Id, ItemType = "产品", ItemId = p6.Id, Code = p6.Code, Name = p6.Name, Specification = p6.Specification, Unit = p6.Unit, Qty = 150, SafeStock = 300 });
            db.SaveChanges();

            // ---------- 库存流水 ----------
            decimal balM1 = 526, balM2 = 450, balM3 = 0, balM4 = 425, balM5 = 0, balM6 = 51;
            var rows = new (DateTime d, string plan, string name, string spec, string bill, decimal? inq, decimal? outq, string remark)[]
            {
                (new DateTime(2025,11,28),"M01","顶盖前横梁板材","1.0*1700*400-HC340/590DP","采购入库",1115,null,"安钢到货"),
                (new DateTime(2025,12,10),"M01","顶盖前横梁板材","1.0*1700*400-HC340/590DP","生产领用",null,300,"车间领料"),
                (new DateTime(2026,1,11),"M01","顶盖前横梁板材","1.0*1700*400-HC340/590DP","生产领用",null,324,"车间领料"),
                (new DateTime(2026,3,23),"M01","顶盖前横梁板材","1.0*1700*400-HC340/590DP","车间入库",600,600,"车间退料+领用"),
                (new DateTime(2026,5,5),"M01","顶盖前横梁板材","1.0*1700*400-HC340/590DP","采购入库",593,null,"安钢到货"),
                (new DateTime(2026,6,17),"M01","顶盖前横梁板材","1.0*1700*400-HC340/590DP","采购入库",617,null,"宝武到货"),
                (new DateTime(2026,7,20),"M01","顶盖前横梁板材","1.0*1700*400-HC340/590DP","采购入库",572,null,"本月到货"),
                (new DateTime(2025,11,28),"M06","行车记录仪后纵梁板材","0.8*880*480-DC01","采购入库",563,563,"采购"),
                (new DateTime(2026,3,30),"M06","行车记录仪后纵梁板材","0.8*880*480-DC01","生产领用",null,440,"车间领料"),
                (new DateTime(2026,7,20),"M06","行车记录仪后纵梁板材","0.8*880*480-DC01","采购入库",603,603,"采购"),
                (new DateTime(2026,5,22),"M45","阅读灯安装支架板材","0.8*480*280-DC51D+Z","采购入库",800,null,"采购"),
                (new DateTime(2026,7,10),"M45","阅读灯安装支架板材","0.8*480*280-DC51D+Z","生产领用",null,480,"车间领料"),
                (new DateTime(2026,4,11),"M51","行车记录仪下安装支架板材","0.8*335*790-DC01","采购入库",100,null,"采购"),
                (new DateTime(2026,7,20),"M51","行车记录仪下安装支架板材","0.8*335*790-DC01","采购入库",100,100,"采购"),
                (new DateTime(2026,5,22),"M55","行车记录仪上安装支架板材","0.8*205*725-DC01","采购入库",50,95,"采购"),
                (new DateTime(2026,7,1),"M57","电视机支架板材","1.5*295*835-HC340/590DP","采购入库",60,60,"采购"),
                (new DateTime(2026,7,15),"M57","电视机支架板材","1.5*295*835-HC340/590DP","生产领用",null,60,"车间领料")
            };
            foreach (var r in rows)
            {
                decimal balance = r.plan switch
                {
                    "M01" => balM1 += (r.inq ?? 0) - (r.outq ?? 0),
                    "M06" => balM2 += (r.inq ?? 0) - (r.outq ?? 0),
                    "M45" => balM3 += (r.inq ?? 0) - (r.outq ?? 0),
                    "M51" => balM4 += (r.inq ?? 0) - (r.outq ?? 0),
                    "M55" => balM5 += (r.inq ?? 0) - (r.outq ?? 0),
                    _ => balM6 += (r.inq ?? 0) - (r.outq ?? 0)
                };
                db.InventoryLedgers.Add(new InventoryLedger { WarehouseId = wRaw.Id, ItemType = "材料", ItemId = 0, ItemName = r.name, Specification = r.spec, BillType = r.bill, BillNo = r.remark, InQty = r.inq ?? 0, OutQty = r.outq ?? 0, BalanceQty = balance, Operator = "admin", OperationTime = r.d });
            }
            db.SaveChanges();

            // ---------- 销售订单 ----------
            var so1 = new SalesOrder { OrderNo = "SO2026080001", CustomerId = c1.Id, OrderDate = new DateTime(2026, 8, 3), DeliveryDate = new DateTime(2026, 8, 25), Status = "已排产", Remark = "月度计划" };
            so1.Items.Add(new SalesOrderItem { ProductId = p1.Id, Qty = 2000, Price = 12.5m, Amount = 25000m, DeliveredQty = 0 });
            so1.Items.Add(new SalesOrderItem { ProductId = p2.Id, Qty = 1500, Price = 8.6m, Amount = 12900m, DeliveredQty = 0 });
            so1.Amount = 37900m;
            var so2 = new SalesOrder { OrderNo = "SO2026080002", CustomerId = c2.Id, OrderDate = new DateTime(2026, 8, 5), DeliveryDate = new DateTime(2026, 8, 30), Status = "确认", Remark = "" };
            so2.Items.Add(new SalesOrderItem { ProductId = p3.Id, Qty = 3000, Price = 5.2m, Amount = 15600m, DeliveredQty = 0 });
            so2.Items.Add(new SalesOrderItem { ProductId = p4.Id, Qty = 1200, Price = 6.8m, Amount = 8160m, DeliveredQty = 0 });
            so2.Amount = 23760m;
            var so3 = new SalesOrder { OrderNo = "SO2026070008", CustomerId = c1.Id, OrderDate = new DateTime(2026, 7, 20), DeliveryDate = new DateTime(2026, 8, 10), Status = "部分发货", Remark = "" };
            so3.Items.Add(new SalesOrderItem { ProductId = p6.Id, Qty = 1000, Price = 9.8m, Amount = 9800m, DeliveredQty = 600 });
            so3.Amount = 9800m;
            db.SalesOrders.AddRange(so1, so2, so3);
            db.SaveChanges();

            // ---------- 发货单 ----------
            var dv1 = new Delivery { DeliveryNo = "DH2026080001", SalesOrderId = so3.Id, CustomerId = c1.Id, WarehouseId = wFin.Id, DeliveryDate = new DateTime(2026, 8, 6), Carrier = "顺丰物流", PlateNo = "豫F12345", Driver = "周师傅", Status = "已发货", Remark = "第一批" };
            dv1.Items.Add(new DeliveryItem { ProductId = p6.Id, Qty = 600, Price = 9.8m });
            db.Deliveries.Add(dv1);

            // ---------- 采购订单 ----------
            var po1 = new PurchaseOrder { OrderNo = "PO2026070003", SupplierId = s1.Id, OrderDate = new DateTime(2026, 7, 20), ExpectDate = new DateTime(2026, 8, 5), Status = "已到货", Amount = 93280m, Remark = "" };
            po1.Items.Add(new PurchaseOrderItem { MaterialId = m1.Id, Qty = 600, Price = 86m, Amount = 51600m, ReceivedQty = 600 });
            po1.Items.Add(new PurchaseOrderItem { MaterialId = m2.Id, Qty = 400, Price = 58m, Amount = 23200m, ReceivedQty = 400 });
            po1.Items.Add(new PurchaseOrderItem { MaterialId = m6.Id, Qty = 200, Price = 93m, Amount = 18600m, ReceivedQty = 200 });
            var po2 = new PurchaseOrder { OrderNo = "PO2026080001", SupplierId = s2.Id, OrderDate = new DateTime(2026, 8, 1), ExpectDate = new DateTime(2026, 8, 20), Status = "已下单", Amount = 24800m, Remark = "" };
            po2.Items.Add(new PurchaseOrderItem { MaterialId = m3.Id, Qty = 400, Price = 42m, Amount = 16800m, ReceivedQty = 0 });
            po2.Items.Add(new PurchaseOrderItem { MaterialId = m5.Id, Qty = 250, Price = 32m, Amount = 8000m, ReceivedQty = 0 });
            db.PurchaseOrders.AddRange(po1, po2);
            db.SaveChanges();

            // ---------- 采购到货单 ----------
            var pr1 = new PurchaseReceipt { ReceiptNo = "SH2026080001", PurchaseOrderId = po1.Id, SupplierId = s1.Id, WarehouseId = wRaw.Id, ReceiptDate = new DateTime(2026, 8, 5), Status = "已入库", Remark = "安钢到货" };
            pr1.Items.Add(new PurchaseReceiptItem { MaterialId = m1.Id, Qty = 600, Price = 86m });
            pr1.Items.Add(new PurchaseReceiptItem { MaterialId = m2.Id, Qty = 400, Price = 58m });
            pr1.Items.Add(new PurchaseReceiptItem { MaterialId = m6.Id, Qty = 200, Price = 93m });
            db.PurchaseReceipts.Add(pr1);
            db.SaveChanges();

            // ---------- 冲压产量单 ----------
            db.ProductionOrders.AddRange(
                new ProductionOrder { Date = new DateTime(2026, 8, 3), PlanNo = "M01", ProcessName = "落料", Project = "顶盖前横梁", ProcessDesc = "剪切落料", FinishedQty = 1200, ScrapQty = 3, CompletedQty = 1203, OrderNo = "CL-0803-01", WorkHours = 8, MachineNo = "EQ003", Operator1 = "张伟", Operator2 = "李强", ShiftId = sh1.Id, ShiftName = "早班" },
                new ProductionOrder { Date = new DateTime(2026, 8, 3), PlanNo = "M01", ProcessName = "拉延", Project = "顶盖前横梁", ProcessDesc = "拉伸成型", FinishedQty = 1150, ScrapQty = 12, CompletedQty = 1162, OrderNo = "LY-0803-01", WorkHours = 8, MachineNo = "EQ001", Operator1 = "李强", Operator2 = "王芳", ShiftId = sh1.Id, ShiftName = "早班" },
                new ProductionOrder { Date = new DateTime(2026, 8, 4), PlanNo = "M01", ProcessName = "修边", Project = "顶盖前横梁", ProcessDesc = "切边", FinishedQty = 1140, ScrapQty = 5, CompletedQty = 1145, OrderNo = "XB-0804-01", WorkHours = 8, MachineNo = "EQ002", Operator1 = "王芳", Operator2 = "张伟", ShiftId = sh2.Id, ShiftName = "中班" },
                new ProductionOrder { Date = new DateTime(2026, 8, 4), PlanNo = "M06", ProcessName = "落料", Project = "行车记录仪后纵梁", ProcessDesc = "剪切落料", FinishedQty = 900, ScrapQty = 2, CompletedQty = 902, OrderNo = "CL-0804-02", WorkHours = 8, MachineNo = "EQ003", Operator1 = "张伟", ShiftId = sh1.Id, ShiftName = "早班" },
                new ProductionOrder { Date = new DateTime(2026, 8, 5), PlanNo = "M06", ProcessName = "拉延", Project = "行车记录仪后纵梁", ProcessDesc = "拉伸成型", FinishedQty = 880, ScrapQty = 8, CompletedQty = 888, OrderNo = "LY-0805-02", WorkHours = 8, MachineNo = "EQ002", Operator1 = "李强", Operator2 = "王芳", ShiftId = sh1.Id, ShiftName = "早班" },
                new ProductionOrder { Date = new DateTime(2026, 8, 5), PlanNo = "M45", ProcessName = "落料", Project = "阅读灯安装支架", ProcessDesc = "剪切落料", FinishedQty = 1500, ScrapQty = 1, CompletedQty = 1501, OrderNo = "CL-0805-03", WorkHours = 8, MachineNo = "EQ003", Operator1 = "王芳", ShiftId = sh2.Id, ShiftName = "中班" },
                new ProductionOrder { Date = new DateTime(2026, 8, 5), PlanNo = "M45", ProcessName = "拉延", Project = "阅读灯安装支架", ProcessDesc = "拉伸成型", FinishedQty = 1480, ScrapQty = 6, CompletedQty = 1486, OrderNo = "LY-0805-03", WorkHours = 8, MachineNo = "EQ001", Operator1 = "张伟", Operator2 = "李强", ShiftId = sh1.Id, ShiftName = "早班" });

            // ---------- 生产日报表 ----------
            var dr1 = new ProductionDailyReport
            {
                ReportDate = new DateTime(2026, 8, 5), PlanNo = "M01", PrevCarryQty = 300, MaterialQty = 1200, BatchNo = "P20260801", ScrapSheets = 2,
                InStockQty = 1120, ShipQty = 1000, SizeSpec = "1.0*1700*400", MaterialSpec = "HC340/590DP", TaiFen = 1,
                TotalLingliao = 1200, TotalFeiliao = 2, TotalChengpin = 1120, TotalFeipin = 12, TotalGongshi = 24, Remark = ""
            };
            dr1.Processes.AddRange(
                new DailyReportProcess { ProcessName = "落料", EquipmentNo = "EQ003", QualifiedQty = 1200, ScrapQty = 3, WorkHours = 8 },
                new DailyReportProcess { ProcessName = "拉延", EquipmentNo = "EQ001", QualifiedQty = 1150, ScrapQty = 12, WorkHours = 8 },
                new DailyReportProcess { ProcessName = "修边", EquipmentNo = "EQ002", QualifiedQty = 1140, ScrapQty = 5, WorkHours = 8 },
                new DailyReportProcess { ProcessName = "冲孔侧冲", EquipmentNo = "EQ002", QualifiedQty = 1120, ScrapQty = 4, WorkHours = 8 });
            db.ProductionDailyReports.Add(dr1);

            // ---------- 质检记录 ----------
            db.QualityInspections.AddRange(
                new QualityInspection { InspectionNo = "QC2026080001", InspectDate = new DateTime(2026, 8, 5), PlanNo = "M01", ProductId = p1.Id, ProcessName = "拉延", InspectQty = 200, QualifiedQty = 190, DefectQty = 10, DefectReason = "表面拉痕", Result = "返工", Inspector = "陈静", Handler = "李强", Remark = "返工后复检" },
                new QualityInspection { InspectionNo = "QC2026080002", InspectDate = new DateTime(2026, 8, 5), PlanNo = "M06", ProductId = p2.Id, ProcessName = "落料", InspectQty = 200, QualifiedQty = 199, DefectQty = 1, DefectReason = "毛刺", Result = "合格", Inspector = "陈静", Handler = "", Remark = "" },
                new QualityInspection { InspectionNo = "QC2026080003", InspectDate = new DateTime(2026, 8, 6), PlanNo = "M45", ProductId = p3.Id, ProcessName = "拉延", InspectQty = 150, QualifiedQty = 146, DefectQty = 4, DefectReason = "皱褶", Result = "不合格", Inspector = "陈静", Handler = "张伟", Remark = "报废处理" },
                new QualityInspection { InspectionNo = "QC2026080004", InspectDate = new DateTime(2026, 8, 6), PlanNo = "M01", ProductId = p1.Id, ProcessName = "修边", InspectQty = 200, QualifiedQty = 198, DefectQty = 2, DefectReason = "尺寸超差", Result = "合格", Inspector = "陈静", Handler = "", Remark = "" });

            // ---------- 计量器具 ----------
            db.MeasuringTools.AddRange(
                new MeasuringTool { ToolNo = "JL-0001", Name = "数显卡尺", Specification = "0-150mm", Qty = 5, Status = "在用", Origin = "桂林广陆", PurchaseDate = new DateTime(2023, 3, 15), UnitPrice = 180, Dept = "质检部", Holder = "陈静", ReceiveDate = new DateTime(2023, 3, 20), CalibrationCycle = "一年", CalibrationPlanDate = new DateTime(2026, 3, 20), CalibrationDate = new DateTime(2026, 3, 18), Remark = "" },
                new MeasuringTool { ToolNo = "JL-0002", Name = "千分尺", Specification = "0-25mm", Qty = 3, Status = "在用", Origin = "成都成量", PurchaseDate = new DateTime(2023, 5, 10), UnitPrice = 260, Dept = "质检部", Holder = "陈静", ReceiveDate = new DateTime(2023, 5, 15), CalibrationCycle = "一年", CalibrationPlanDate = new DateTime(2026, 5, 15), Remark = "" },
                new MeasuringTool { ToolNo = "JL-0003", Name = "三坐标测量机", Specification = "600*800*600", Qty = 1, Status = "在用", Origin = "海克斯康", PurchaseDate = new DateTime(2022, 6, 1), UnitPrice = 350000, Dept = "质检部", Holder = "刘工", ReceiveDate = new DateTime(2022, 6, 10), CalibrationCycle = "一年", CalibrationPlanDate = new DateTime(2026, 6, 10), Remark = "重点设备" },
                new MeasuringTool { ToolNo = "JL-0004", Name = "粗糙度仪", Specification = "Ra 0.05-6.3", Qty = 1, Status = "待检", Origin = "北京时代", PurchaseDate = new DateTime(2023, 8, 1), UnitPrice = 8000, Dept = "质检部", Holder = "陈静", ReceiveDate = new DateTime(2023, 8, 5), CalibrationCycle = "半年", CalibrationPlanDate = new DateTime(2026, 2, 5), CalibrationDate = new DateTime(2026, 1, 30), Remark = "待校准" });

            // ---------- 量具申购/报废/检定 ----------
            db.ToolApplies.AddRange(
                new ToolApply { ApplyNo = "SJ2026070001", Name = "深度卡尺", Specification = "0-200mm", Qty = 2, Reason = "新品零件测量", Dept = "质检部", ApplyDate = new DateTime(2026, 7, 10), ArrivalDate = new DateTime(2026, 7, 25), AuditStatus = "同意", Remark = "" },
                new ToolApply { ApplyNo = "SJ2026080001", Name = "塞规", Specification = "Φ10H7", Qty = 10, Reason = "孔位检测", Dept = "冲压车间", ApplyDate = new DateTime(2026, 8, 2), ArrivalDate = null, AuditStatus = "待审核", Remark = "" });
            db.ToolScraps.Add(new ToolScrap { ScrapNo = "BF2026070001", ToolName = "游标卡尺", Specification = "0-300mm", ManageNo = "JL-0012", FactoryNo = "CB20180012", Manufacturer = "广陆", Holder = "李强", Qty = 1, ReceiveDate = new DateTime(2019, 5, 1), ScrapDate = new DateTime(2026, 7, 5), Reason = "测量面磨损超差", Applicant = "陈静", Approver = "质量主管" });
            db.ToolCalibrations.Add(new ToolCalibration { CalibrationNo = "JD2026080001", ToolName = "数显卡尺", MeasureRange = "0-150mm", ToolNo = "JL-0001", Origin = "桂林广陆", ReceiveDate = new DateTime(2026, 8, 1), Dept = "质检部", UserName = "陈静", Result = "合格", AnomalyDesc = "", HandleAdvice = "", ReviewAdvice = "继续使用", Reviewer = "质量主管", ReviewDate = new DateTime(2026, 8, 3) });

            // ---------- 模具台账 ----------
            db.Molds.AddRange(
                new Mold { MoldNo = "M01", Name = "顶盖前横梁冲压模", CustomerId = c1.Id, ProjectName = "顶盖前横梁项目", PlanNo = "M01", ProcessType = "落料+拉延复合模", Tonnage = 250, Status = "量产", Location = "A区01号", Manager = "刘洋", ProductId = p1.Id },
                new Mold { MoldNo = "M06", Name = "行车记录仪后纵梁冲压模", CustomerId = c1.Id, ProjectName = "行车记录仪后纵梁项目", PlanNo = "M06", ProcessType = "拉延模", Tonnage = 315, Status = "量产", Location = "A区02号", Manager = "刘洋", ProductId = p2.Id },
                new Mold { MoldNo = "M45", Name = "阅读灯安装支架冲压模", CustomerId = c2.Id, ProjectName = "阅读灯安装支架项目", PlanNo = "M45", ProcessType = "落料+拉延复合模", Tonnage = 250, Status = "量产", Location = "B区03号", Manager = "刘洋", ProductId = p3.Id },
                new Mold { MoldNo = "M88", Name = "新项目保险杠冲压模", CustomerId = c3.Id, ProjectName = "保险杠支架项目", PlanNo = "M88", ProcessType = "拉延模", Tonnage = 400, Status = "制造中", Location = "模具车间", Manager = "刘洋" });

            // ---------- 模具制造计划（钢板模） ----------
            var mp1 = new MoldPlan { PlanNo = "MJ2026-001", CustomerId = c3.Id, ProjectName = "保险杠支架项目", MoldNo = "M88", MoldName = "保险杠支架冲压模", ProcessName = "落料+拉延", Tonnage = 400, MoldStatus = "排产中", PlanArrival = new DateTime(2026, 10, 30), ActualArrival = null, Remark = "新订单" };
            var stages = new[] { "编程2D", "编程3D", "2D加工", "淬火计划", "投线", "线切割", "机钳装配", "3D精加工", "合模装配", "研合完成", "调试完成" };
            var baseDate = new DateTime(2026, 8, 10);
            for (int i = 0; i < stages.Length; i++)
                mp1.Stages.Add(new MoldPlanStage { StageName = stages[i], PlanStart = baseDate.AddDays(i * 7), PlanEnd = baseDate.AddDays(i * 7 + 6), Status = i < 2 ? "进行中" : "未开始" });
            db.MoldPlans.Add(mp1);

            // ---------- 排班 ----------
            db.WorkSchedules.AddRange(
                new WorkSchedule { WorkDate = new DateTime(2026, 8, 10), EmployeeId = e1.Id, ShiftId = sh1.Id, Workshop = "冲压车间", Task = "M01拉延生产" },
                new WorkSchedule { WorkDate = new DateTime(2026, 8, 10), EmployeeId = e2.Id, ShiftId = sh1.Id, Workshop = "冲压车间", Task = "M01拉延生产" },
                new WorkSchedule { WorkDate = new DateTime(2026, 8, 10), EmployeeId = e3.Id, ShiftId = sh2.Id, Workshop = "冲压车间", Task = "M45落料生产" },
                new WorkSchedule { WorkDate = new DateTime(2026, 8, 11), EmployeeId = e4.Id, ShiftId = sh1.Id, Workshop = "模具车间", Task = "M88模具编程" });

            // ---------- 设备维护记录 ----------
            db.EquipmentMaintenances.AddRange(
                new EquipmentMaintenance { EquipmentId = eq1.Id, MaintainDate = new DateTime(2026, 7, 15), Type = "保养", Content = "液压油更换、滑块导轨润滑", Cost = 1500, Handler = "维修班", Result = "正常" },
                new EquipmentMaintenance { EquipmentId = eq5.Id, MaintainDate = new DateTime(2026, 8, 1), Type = "维修", Content = "主轴轴承异响检修", Cost = 3200, Handler = "维修班", Result = "维修中" });

            db.SaveChanges();
        }
    }
}
