using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionPlansController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProductionPlansController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var q = _db.ProductionPlans.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.PlanNo.Contains(keyword) || p.ProjectName.Contains(keyword) || (p.Customer != null && p.Customer.Name.Contains(keyword)));
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(p => p.Status == status);
            var list = await q.OrderBy(p => p.PlanNo).Select(p => new
            {
                p.Id, p.PlanNo, p.ProjectName, p.PlanQty, p.OneOutputs, p.PlannedStart, p.PlannedEnd,
                p.ActualEnd, p.Status, p.Remark,
                CustomerName = p.Customer != null ? p.Customer.Name : "",
                ProductName = p.Product != null ? p.Product.Name : "",
                ProductSpec = p.Product != null ? p.Product.Specification : "",
                MaterialName = p.Material != null ? p.Material.Name : "",
                MaterialSpec = p.Material != null ? p.Material.Specification : "",
                DoneQty = _db.ProductionOrders.Where(o => o.PlanNo == p.PlanNo).Sum(o => (decimal?)o.CompletedQty) ?? 0
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _db.ProductionPlans
                .Include(x => x.Customer).Include(x => x.Product).Include(x => x.Material)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return Ok(new
            {
                p.Id, p.PlanNo, p.ProjectName, p.PlanQty, p.OneOutputs, p.PlannedStart, p.PlannedEnd,
                p.ActualEnd, p.Status, p.Remark, p.CustomerId, CustomerName = p.Customer?.Name ?? "",
                p.ProductId, ProductName = p.Product?.Name ?? "", ProductSpec = p.Product?.Specification ?? "",
                p.MaterialId, MaterialName = p.Material?.Name ?? "", MaterialSpec = p.Material?.Specification ?? ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductionPlan plan)
        {
            if (string.IsNullOrEmpty(plan.PlanNo))
                plan.PlanNo = "M" + DateTime.Now.ToString("yyMM") + (await _db.ProductionPlans.CountAsync() + 1);
            _db.ProductionPlans.Add(plan);
            await _db.SaveChangesAsync();
            return Ok(plan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductionPlan input)
        {
            var p = await _db.ProductionPlans.FindAsync(id);
            if (p == null) return NotFound();
            p.PlanNo = input.PlanNo;
            p.CustomerId = input.CustomerId;
            p.ProjectName = input.ProjectName;
            p.ProductId = input.ProductId;
            p.MaterialId = input.MaterialId;
            p.OneOutputs = input.OneOutputs;
            p.PlanQty = input.PlanQty;
            p.PlannedStart = input.PlannedStart;
            p.PlannedEnd = input.PlannedEnd;
            p.ActualEnd = input.ActualEnd;
            p.Status = input.Status;
            p.Remark = input.Remark;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] string status)
        {
            var p = await _db.ProductionPlans.FindAsync(id);
            if (p == null) return NotFound();
            p.Status = status;
            if (status == "已完成") p.ActualEnd = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(new { message = "状态更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.ProductionPlans.FindAsync(id);
            if (p == null) return NotFound();
            _db.ProductionPlans.Remove(p);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionOrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProductionOrdersController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? planNo, [FromQuery] string? start, [FromQuery] string? end)
        {
            var q = _db.ProductionOrders.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(o => o.PlanNo.Contains(keyword) || o.Project.Contains(keyword) || o.OrderNo.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(planNo)) q = q.Where(o => o.PlanNo == planNo);
            if (DateTime.TryParse(start, out var d1)) q = q.Where(o => o.Date >= d1);
            if (DateTime.TryParse(end, out var d2)) q = q.Where(o => o.Date <= d2);
            var list = await q.OrderByDescending(o => o.Date).ThenByDescending(o => o.Id).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductionOrder order)
        {
            if (order.CompletedQty == 0) order.CompletedQty = order.FinishedQty + order.ScrapQty;
            if (string.IsNullOrEmpty(order.OrderNo))
                order.OrderNo = "SC" + DateTime.Now.ToString("yyyyMMddHHmmss");
            _db.ProductionOrders.Add(order);
            await _db.SaveChangesAsync();
            return Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductionOrder input)
        {
            var o = await _db.ProductionOrders.FindAsync(id);
            if (o == null) return NotFound();
            o.Date = input.Date;
            o.PlanNo = input.PlanNo;
            o.ProcessName = input.ProcessName;
            o.Project = input.Project;
            o.ProcessDesc = input.ProcessDesc;
            o.FinishedQty = input.FinishedQty;
            o.ScrapQty = input.ScrapQty;
            o.CompletedQty = input.CompletedQty == 0 ? input.FinishedQty + input.ScrapQty : input.CompletedQty;
            o.OrderNo = input.OrderNo;
            o.WorkHours = input.WorkHours;
            o.MachineNo = input.MachineNo;
            o.Operator1 = input.Operator1;
            o.Operator2 = input.Operator2;
            o.Operator3 = input.Operator3;
            o.Operator4 = input.Operator4;
            o.ShiftId = input.ShiftId;
            o.ShiftName = input.ShiftName;
            o.Remark = input.Remark;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var o = await _db.ProductionOrders.FindAsync(id);
            if (o == null) return NotFound();
            _db.ProductionOrders.Remove(o);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionDailyReportsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProductionDailyReportsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? start, [FromQuery] string? end)
        {
            var q = _db.ProductionDailyReports.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword)) q = q.Where(r => r.PlanNo.Contains(keyword));
            if (DateTime.TryParse(start, out var d1)) q = q.Where(r => r.ReportDate >= d1);
            if (DateTime.TryParse(end, out var d2)) q = q.Where(r => r.ReportDate <= d2);
            var list = await q.OrderByDescending(r => r.ReportDate).ThenByDescending(r => r.Id)
                .Select(r => new
                {
                    r.Id, r.ReportDate, r.PlanNo, r.PrevCarryQty, r.MaterialQty, r.BatchNo, r.ScrapSheets,
                    r.InStockQty, r.ShipQty, r.SizeSpec, r.MaterialSpec, r.TaiFen,
                    r.TotalLingliao, r.TotalFeiliao, r.TotalChengpin, r.TotalFeipin, r.TotalGongshi, r.Remark,
                    Processes = r.Processes.Select(p => new { p.Id, p.ProcessName, p.EquipmentNo, p.QualifiedQty, p.ScrapQty, p.WorkHours }).ToList()
                }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var r = await _db.ProductionDailyReports.Include(x => x.Processes).FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductionDailyReport report)
        {
            _db.ProductionDailyReports.Add(report);
            await _db.SaveChangesAsync();
            return Ok(report);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductionDailyReport input)
        {
            var r = await _db.ProductionDailyReports.Include(x => x.Processes).FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            r.ReportDate = input.ReportDate;
            r.PlanNo = input.PlanNo;
            r.PrevCarryQty = input.PrevCarryQty;
            r.MaterialQty = input.MaterialQty;
            r.BatchNo = input.BatchNo;
            r.ScrapSheets = input.ScrapSheets;
            r.InStockQty = input.InStockQty;
            r.ShipQty = input.ShipQty;
            r.SizeSpec = input.SizeSpec;
            r.MaterialSpec = input.MaterialSpec;
            r.TaiFen = input.TaiFen;
            r.TotalLingliao = input.TotalLingliao;
            r.TotalFeiliao = input.TotalFeiliao;
            r.TotalChengpin = input.TotalChengpin;
            r.TotalFeipin = input.TotalFeipin;
            r.TotalGongshi = input.TotalGongshi;
            r.Remark = input.Remark;
            _db.DailyReportProcesses.RemoveRange(r.Processes);
            r.Processes.Clear();
            foreach (var p in input.Processes) r.Processes.Add(new DailyReportProcess
            {
                ProcessName = p.ProcessName, EquipmentNo = p.EquipmentNo,
                QualifiedQty = p.QualifiedQty, ScrapQty = p.ScrapQty, WorkHours = p.WorkHours
            });
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _db.ProductionDailyReports.Include(x => x.Processes).FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            _db.DailyReportProcesses.RemoveRange(r.Processes);
            _db.ProductionDailyReports.Remove(r);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }
}
