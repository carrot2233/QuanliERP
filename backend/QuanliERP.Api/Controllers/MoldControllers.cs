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
    public class MoldsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MoldsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var q = _db.Molds.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(m => m.MoldNo.Contains(keyword) || m.Name.Contains(keyword) || m.ProjectName.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(m => m.Status == status);
            var list = await q.OrderBy(m => m.MoldNo).Select(m => new
            {
                m.Id, m.MoldNo, m.Name, m.ProjectName, m.PlanNo, m.ProcessType, m.Tonnage, m.Status,
                m.Location, m.Manager, m.Remark,
                CustomerName = m.Customer != null ? m.Customer.Name : "",
                ProductName = m.Product != null ? m.Product.Name : ""
            }).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Mold item)
        {
            _db.Molds.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Mold input)
        {
            var x = await _db.Molds.FindAsync(id);
            if (x == null) return NotFound();
            x.MoldNo = input.MoldNo;
            x.Name = input.Name;
            x.CustomerId = input.CustomerId;
            x.ProjectName = input.ProjectName;
            x.PlanNo = input.PlanNo;
            x.ProcessType = input.ProcessType;
            x.Tonnage = input.Tonnage;
            x.Status = input.Status;
            x.Location = input.Location;
            x.Manager = input.Manager;
            x.ProductId = input.ProductId;
            x.Remark = input.Remark;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.Molds.FindAsync(id);
            if (x == null) return NotFound();
            _db.Molds.Remove(x);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MoldPlansController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MoldPlansController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword)
        {
            var q = _db.MoldPlans.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.PlanNo.Contains(keyword) || p.MoldNo.Contains(keyword) || p.MoldName.Contains(keyword) || p.ProjectName.Contains(keyword));
            var list = await q.OrderByDescending(p => p.Id).Select(p => new
            {
                p.Id, p.PlanNo, p.ProjectName, p.MoldNo, p.MoldName, p.ProcessName, p.Tonnage,
                p.MoldStatus, p.PlanArrival, p.ActualArrival, p.Remark,
                CustomerName = p.Customer != null ? p.Customer.Name : "",
                DoneStages = p.Stages.Count(s => s.Status == "已完成"),
                TotalStages = p.Stages.Count,
                Progress = p.Stages.Count == 0 ? 0 : (decimal)p.Stages.Count(s => s.Status == "已完成") * 100 / p.Stages.Count,
                Stages = p.Stages.OrderBy(s => s.Id).Select(s => new { s.Id, s.StageName, s.PlanStart, s.PlanEnd, s.ActualStart, s.ActualEnd, s.Status, s.Remark }).ToList()
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _db.MoldPlans.Include(x => x.Stages).Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return Ok(new
            {
                p.Id, p.PlanNo, p.ProjectName, p.MoldNo, p.MoldName, p.ProcessName, p.Tonnage,
                p.MoldStatus, p.PlanArrival, p.ActualArrival, p.Remark, p.CustomerId, CustomerName = p.Customer?.Name ?? "",
                Stages = p.Stages.OrderBy(s => s.Id).Select(s => new { s.Id, s.StageName, s.PlanStart, s.PlanEnd, s.ActualStart, s.ActualEnd, s.Status, s.Remark }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(MoldPlan plan)
        {
            if (string.IsNullOrEmpty(plan.PlanNo))
                plan.PlanNo = "MJ" + DateTime.Now.ToString("yyyyMMddHHmmss");
            _db.MoldPlans.Add(plan);
            await _db.SaveChangesAsync();
            return Ok(plan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MoldPlan input)
        {
            var p = await _db.MoldPlans.Include(x => x.Stages).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            p.PlanNo = input.PlanNo;
            p.CustomerId = input.CustomerId;
            p.ProjectName = input.ProjectName;
            p.MoldNo = input.MoldNo;
            p.MoldName = input.MoldName;
            p.ProcessName = input.ProcessName;
            p.Tonnage = input.Tonnage;
            p.MoldStatus = input.MoldStatus;
            p.PlanArrival = input.PlanArrival;
            p.ActualArrival = input.ActualArrival;
            p.Remark = input.Remark;

            foreach (var old in p.Stages.ToList())
            {
                var match = input.Stages.FirstOrDefault(s => s.Id == old.Id);
                if (match == null) _db.MoldPlanStages.Remove(old);
                else
                {
                    old.StageName = match.StageName;
                    old.PlanStart = match.PlanStart;
                    old.PlanEnd = match.PlanEnd;
                    old.ActualStart = match.ActualStart;
                    old.ActualEnd = match.ActualEnd;
                    old.Status = match.Status;
                    old.Remark = match.Remark;
                }
            }
            foreach (var s in input.Stages.Where(x => x.Id == 0))
            {
                p.Stages.Add(new MoldPlanStage
                {
                    StageName = s.StageName, PlanStart = s.PlanStart, PlanEnd = s.PlanEnd,
                    ActualStart = s.ActualStart, ActualEnd = s.ActualEnd, Status = s.Status, Remark = s.Remark
                });
            }
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.MoldPlans.Include(x => x.Stages).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            _db.MoldPlanStages.RemoveRange(p.Stages);
            _db.MoldPlans.Remove(p);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }
}
