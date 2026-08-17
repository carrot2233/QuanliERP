using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;

namespace QuanliERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) { _db = db; }

        [HttpGet("overview")]
        public async Task<IActionResult> Overview()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var todayOutput = await _db.ProductionOrders
                .Where(o => o.Date == today).SumAsync(o => (decimal?)o.CompletedQty) ?? 0;
            var monthOutput = await _db.ProductionOrders
                .Where(o => o.Date >= monthStart && o.Date <= monthEnd).SumAsync(o => (decimal?)o.CompletedQty) ?? 0;
            var monthSales = await _db.SalesOrders
                .Where(o => o.OrderDate >= monthStart && o.OrderDate <= monthEnd).SumAsync(o => (decimal?)o.Amount) ?? 0;
            var monthPurchase = await _db.PurchaseOrders
                .Where(o => o.OrderDate >= monthStart && o.OrderDate <= monthEnd).SumAsync(o => (decimal?)o.Amount) ?? 0;
            var warningCount = await _db.Inventories.CountAsync(i => i.Qty <= i.SafeStock);
            var runningPlans = await _db.ProductionPlans.CountAsync(p => p.Status == "进行中");
            var inProduction = await _db.ProductionPlans.CountAsync(p => p.Status == "未开始" || p.Status == "进行中");
            var equipmentRunning = await _db.Equipments.CountAsync(e => e.Status == "运行");
            var equipmentTotal = await _db.Equipments.CountAsync();
            var todayDelivery = await _db.Deliveries.CountAsync(d => d.DeliveryDate == today);
            var monthDelivery = await _db.Deliveries
                .Where(d => d.DeliveryDate >= monthStart && d.DeliveryDate <= monthEnd).CountAsync();

            return Ok(new
            {
                TodayOutput = todayOutput,
                MonthOutput = monthOutput,
                MonthSales = monthSales,
                MonthPurchase = monthPurchase,
                WarningCount = warningCount,
                RunningPlans = runningPlans,
                InProduction = inProduction,
                EquipmentRunning = equipmentRunning,
                EquipmentTotal = equipmentTotal,
                TodayDelivery = todayDelivery,
                MonthDelivery = monthDelivery,
                MaterialInventory = await _db.Inventories.Where(i => i.ItemType == "材料").SumAsync(i => (decimal?)i.Qty) ?? 0,
                ProductInventory = await _db.Inventories.Where(i => i.ItemType == "产品").SumAsync(i => (decimal?)i.Qty) ?? 0,
                MeasuringToolCount = await _db.MeasuringTools.CountAsync(),
                MoldCount = await _db.Molds.CountAsync()
            });
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> Inventory()
        {
            var list = await _db.Inventories.ToListAsync();
            var materials = list.Where(i => i.ItemType == "材料").ToList();
            var products = list.Where(i => i.ItemType == "产品").ToList();
            return Ok(new
            {
                MaterialTotal = materials.Sum(i => i.Qty),
                ProductTotal = products.Sum(i => i.Qty),
                MaterialWarning = materials.Count(i => i.Qty <= i.SafeStock),
                ProductWarning = products.Count(i => i.Qty <= i.SafeStock),
                Normal = list.Count(i => i.Qty > i.SafeStock),
                Warning = list.Count(i => i.Qty <= i.SafeStock && i.Qty > 0),
                OutOfStock = list.Count(i => i.Qty <= 0),
                WarehouseStock = await _db.Inventories.GroupBy(i => i.Warehouse != null ? i.Warehouse.Name : "未知仓库")
                    .Select(g => new { Name = g.Key, Value = g.Sum(i => i.Qty) }).ToListAsync()
            });
        }

        [HttpGet("production-progress")]
        public async Task<IActionResult> ProductionProgress()
        {
            var plans = await _db.ProductionPlans.ToListAsync();
            var result = new List<object>();
            foreach (var p in plans)
            {
                var done = await _db.ProductionOrders.Where(o => o.PlanNo == p.PlanNo)
                    .SumAsync(o => (decimal?)o.CompletedQty) ?? 0;
                result.Add(new
                {
                    p.PlanNo, p.ProjectName, p.PlanQty, Done = done,
                    Progress = p.PlanQty <= 0 ? 0 : Math.Round(done * 100 / p.PlanQty, 1),
                    p.Status
                });
            }
            return Ok(result);
        }

        [HttpGet("quality")]
        public async Task<IActionResult> Quality()
        {
            var inspections = await _db.QualityInspections.ToListAsync();
            var trend = inspections.GroupBy(x => x.InspectDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    InspectQty = g.Sum(x => x.InspectQty),
                    DefectQty = g.Sum(x => x.DefectQty),
                    QualifiedQty = g.Sum(x => x.QualifiedQty),
                    PassRate = g.Sum(x => x.InspectQty) == 0 ? 100 : Math.Round((decimal)g.Sum(x => x.QualifiedQty) * 100 / g.Sum(x => x.InspectQty), 1)
                }).OrderBy(x => x.Date).ToList();

            var reasons = inspections.Where(x => !string.IsNullOrWhiteSpace(x.DefectReason))
                .GroupBy(x => x.DefectReason)
                .Select(g => new { Name = g.Key, Value = g.Sum(x => x.DefectQty) })
                .OrderByDescending(x => x.Value).Take(8).ToList();

            var summary = new
            {
                TotalInspect = inspections.Sum(x => x.InspectQty),
                TotalQualified = inspections.Sum(x => x.QualifiedQty),
                TotalDefect = inspections.Sum(x => x.DefectQty),
                PassRate = inspections.Sum(x => x.InspectQty) == 0 ? 100 : Math.Round((decimal)inspections.Sum(x => x.QualifiedQty) * 100 / inspections.Sum(x => x.InspectQty), 1),
                PassCount = inspections.Count(x => x.Result == "合格"),
                FailCount = inspections.Count(x => x.Result == "不合格"),
                ReworkCount = inspections.Count(x => x.Result == "返工")
            };
            return Ok(new { summary, trend, reasons });
        }

        [HttpGet("sales-trend")]
        public async Task<IActionResult> SalesTrend()
        {
            var today = DateTime.Today;
            var start = new DateTime(today.Year, 1, 1);
            var sales = await _db.SalesOrders.Where(o => o.OrderDate >= start)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Amount = g.Sum(o => o.Amount) })
                .ToListAsync();
            var purchase = await _db.PurchaseOrders.Where(o => o.OrderDate >= start)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Amount = g.Sum(o => o.Amount) })
                .ToListAsync();

            var months = Enumerable.Range(1, today.Month)
                .Select(m => $"{today.Year}-{m:D2}").ToList();
            var salesData = months.Select(m =>
            {
                var y = int.Parse(m.Split('-')[0]);
                var mo = int.Parse(m.Split('-')[1]);
                return sales.FirstOrDefault(x => x.Year == y && x.Month == mo)?.Amount ?? 0;
            }).ToList();
            var purchaseData = months.Select(m =>
            {
                var y = int.Parse(m.Split('-')[0]);
                var mo = int.Parse(m.Split('-')[1]);
                return purchase.FirstOrDefault(x => x.Year == y && x.Month == mo)?.Amount ?? 0;
            }).ToList();

            return Ok(new { months, sales = salesData, purchase = purchaseData });
        }

        [HttpGet("production-trend")]
        public async Task<IActionResult> ProductionTrend()
        {
            var today = DateTime.Today;
            var start = new DateTime(today.Year, 1, 1);
            var data = await _db.ProductionOrders.Where(o => o.Date >= start)
                .GroupBy(o => new { o.Date.Year, o.Date.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Qty = g.Sum(o => o.CompletedQty), Scrap = g.Sum(o => o.ScrapQty) })
                .ToListAsync();
            var months = Enumerable.Range(1, today.Month).Select(m => $"{today.Year}-{m:D2}").ToList();
            var qty = months.Select(m =>
            {
                var y = int.Parse(m.Split('-')[0]); var mo = int.Parse(m.Split('-')[1]);
                return data.FirstOrDefault(x => x.Year == y && x.Month == mo)?.Qty ?? 0;
            }).ToList();
            var scrap = months.Select(m =>
            {
                var y = int.Parse(m.Split('-')[0]); var mo = int.Parse(m.Split('-')[1]);
                return data.FirstOrDefault(x => x.Year == y && x.Month == mo)?.Scrap ?? 0;
            }).ToList();
            return Ok(new { months, qty, scrap });
        }

        [HttpGet("process-distribution")]
        public async Task<IActionResult> ProcessDistribution()
        {
            var data = await _db.ProductionOrders.GroupBy(o => o.ProcessName)
                .Select(g => new { Name = g.Key, Value = g.Sum(o => o.CompletedQty) }).ToListAsync();
            return Ok(data);
        }

        [HttpGet("recent-activities")]
        public async Task<IActionResult> RecentActivities()
        {
            var orders = await _db.SalesOrders.OrderByDescending(o => o.CreatedAt).Take(5)
                .Select(o => new { Time = o.CreatedAt, Title = $"销售订单 {o.OrderNo}", Desc = $"金额 {o.Amount}" }).ToListAsync();
            var receipts = await _db.PurchaseReceipts.OrderByDescending(r => r.CreatedAt).Take(5)
                .Select(r => new { Time = r.CreatedAt, Title = $"采购到货 {r.ReceiptNo}", Desc = r.Remark }).ToListAsync();
            var qc = await _db.QualityInspections.OrderByDescending(x => x.CreatedAt).Take(5)
                .Select(x => new { Time = x.CreatedAt, Title = $"质检 {x.InspectionNo}", Desc = $"{x.Result} / 检验{x.InspectQty}" }).ToListAsync();
            var all = orders.Concat(receipts).Concat(qc)
                .OrderByDescending(x => x.Time).Take(10).ToList();
            return Ok(all);
        }
    }
}
