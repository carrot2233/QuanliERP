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
    public class QualityInspectionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public QualityInspectionsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? result)
        {
            var q = _db.QualityInspections.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(x => x.InspectionNo.Contains(keyword) || x.PlanNo.Contains(keyword) || (x.Product != null && x.Product.Name.Contains(keyword)));
            if (!string.IsNullOrWhiteSpace(result)) q = q.Where(x => x.Result == result);
            var list = await q.OrderByDescending(x => x.InspectDate).Select(x => new
            {
                x.Id, x.InspectionNo, x.InspectDate, x.PlanNo, x.ProcessName, x.InspectQty, x.QualifiedQty,
                x.DefectQty, x.DefectReason, x.Result, x.Inspector, x.Handler, x.Remark,
                ProductName = x.Product != null ? x.Product.Name : ""
            }).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(QualityInspection item)
        {
            if (string.IsNullOrEmpty(item.InspectionNo))
                item.InspectionNo = "QC" + DateTime.Now.ToString("yyyyMMddHHmmss");
            _db.QualityInspections.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, QualityInspection input)
        {
            var x = await _db.QualityInspections.FindAsync(id);
            if (x == null) return NotFound();
            x.InspectionNo = input.InspectionNo;
            x.InspectDate = input.InspectDate;
            x.PlanNo = input.PlanNo;
            x.ProductId = input.ProductId;
            x.ProcessName = input.ProcessName;
            x.InspectQty = input.InspectQty;
            x.QualifiedQty = input.QualifiedQty;
            x.DefectQty = input.DefectQty;
            x.DefectReason = input.DefectReason;
            x.Result = input.Result;
            x.Inspector = input.Inspector;
            x.Handler = input.Handler;
            x.Remark = input.Remark;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.QualityInspections.FindAsync(id);
            if (x == null) return NotFound();
            _db.QualityInspections.Remove(x);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MeasuringToolsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MeasuringToolsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var q = _db.MeasuringTools.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(t => t.ToolNo.Contains(keyword) || t.Name.Contains(keyword) || t.Specification.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(t => t.Status == status);
            var list = await q.OrderBy(t => t.ToolNo).ToListAsync();
            return Ok(list);
        }

        [HttpGet("calibration-overdue")]
        public async Task<IActionResult> GetCalibrationOverdue()
        {
            var today = DateTime.Today;
            var list = await _db.MeasuringTools
                .Where(t => t.CalibrationPlanDate != null && t.CalibrationPlanDate < today)
                .OrderBy(t => t.CalibrationPlanDate).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MeasuringTool item)
        {
            _db.MeasuringTools.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MeasuringTool input)
        {
            var x = await _db.MeasuringTools.FindAsync(id);
            if (x == null) return NotFound();
            x.ToolNo = input.ToolNo;
            x.Name = input.Name;
            x.Specification = input.Specification;
            x.Qty = input.Qty;
            x.Status = input.Status;
            x.Origin = input.Origin;
            x.PurchaseDate = input.PurchaseDate;
            x.UnitPrice = input.UnitPrice;
            x.Dept = input.Dept;
            x.Holder = input.Holder;
            x.ReceiveDate = input.ReceiveDate;
            x.CalibrationCycle = input.CalibrationCycle;
            x.CalibrationPlanDate = input.CalibrationPlanDate;
            x.CalibrationDate = input.CalibrationDate;
            x.StopDate = input.StopDate;
            x.Remark = input.Remark;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.MeasuringTools.FindAsync(id);
            if (x == null) return NotFound();
            _db.MeasuringTools.Remove(x);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    [Route("api/[controller]")]
    public class ToolAppliesController : CrudBaseController<ToolApply>
    {
        public ToolAppliesController(AppDbContext db) : base(db) { }
        protected override void PrepareNew(ToolApply item)
        {
            if (string.IsNullOrEmpty(item.ApplyNo))
                item.ApplyNo = "SQ" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }

    [Route("api/[controller]")]
    public class ToolScrapsController : CrudBaseController<ToolScrap>
    {
        public ToolScrapsController(AppDbContext db) : base(db) { }
        protected override void PrepareNew(ToolScrap item)
        {
            if (string.IsNullOrEmpty(item.ScrapNo))
                item.ScrapNo = "BF" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }

    [Route("api/[controller]")]
    public class ToolCalibrationsController : CrudBaseController<ToolCalibration>
    {
        public ToolCalibrationsController(AppDbContext db) : base(db) { }
        protected override void PrepareNew(ToolCalibration item)
        {
            if (string.IsNullOrEmpty(item.CalibrationNo))
                item.CalibrationNo = "JD" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}
