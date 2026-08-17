using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Controllers
{
    [Route("api/[controller]")]
    public class ShiftsController : CrudBaseController<Shift>
    {
        public ShiftsController(AppDbContext db) : base(db) { }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkSchedulesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public WorkSchedulesController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? start, [FromQuery] string? end, [FromQuery] string? keyword)
        {
            var q = _db.WorkSchedules.AsQueryable();
            if (DateTime.TryParse(start, out var d1)) q = q.Where(s => s.WorkDate >= d1);
            if (DateTime.TryParse(end, out var d2)) q = q.Where(s => s.WorkDate <= d2);
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(s => s.Workshop.Contains(keyword) || (s.Employee != null && s.Employee.Name.Contains(keyword)));
            var list = await q.OrderBy(s => s.WorkDate).ThenBy(s => s.ShiftId).Select(s => new
            {
                s.Id, s.WorkDate, s.EmployeeId, EmployeeName = s.Employee != null ? s.Employee.Name : "",
                s.ShiftId, ShiftName = s.Shift != null ? s.Shift.Name : "",
                ShiftStart = s.Shift != null ? s.Shift.StartTime : "",
                ShiftEnd = s.Shift != null ? s.Shift.EndTime : "",
                s.Workshop, s.Task, s.Remark
            }).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkSchedule item)
        {
            _db.WorkSchedules.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, WorkSchedule input)
        {
            var x = await _db.WorkSchedules.FindAsync(id);
            if (x == null) return NotFound();
            x.WorkDate = input.WorkDate;
            x.EmployeeId = input.EmployeeId;
            x.ShiftId = input.ShiftId;
            x.Workshop = input.Workshop;
            x.Task = input.Task;
            x.Remark = input.Remark;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.WorkSchedules.FindAsync(id);
            if (x == null) return NotFound();
            _db.WorkSchedules.Remove(x);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }
}
