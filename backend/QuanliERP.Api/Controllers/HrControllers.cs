using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Controllers
{
    [Route("api/[controller]")]
    public class AttendancesController : CrudBaseController<Attendance>
    {
        public AttendancesController(AppDbContext db) : base(db) { }
    }

    [Route("api/[controller]")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public LeaveRequestsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var q = _db.LeaveRequests.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(l => l.LeaveNo.Contains(keyword) || l.EmpName.Contains(keyword) || l.EmpCode.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(l => l.Status == status);
            var list = await q.OrderByDescending(l => l.CreatedAt).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var x = await _db.LeaveRequests.FindAsync(id);
            if (x == null) return NotFound();
            return Ok(x);
        }

        [HttpPost]
        public async Task<IActionResult> Create(LeaveRequest item)
        {
            if (string.IsNullOrEmpty(item.LeaveNo))
                item.LeaveNo = "QJ" + DateTime.Now.ToString("yyyyMMddHHmmss");
            _db.LeaveRequests.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, LeaveRequest input)
        {
            var x = await _db.LeaveRequests.FindAsync(id);
            if (x == null) return NotFound();
            x.EmpCode = input.EmpCode;
            x.EmpName = input.EmpName;
            x.LeaveType = input.LeaveType;
            x.StartDate = input.StartDate;
            x.EndDate = input.EndDate;
            x.Days = input.Days;
            x.Reason = input.Reason;
            x.Status = input.Status;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        // 审批请假单
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id, [FromBody] LeaveApproveDto dto)
        {
            var x = await _db.LeaveRequests.FindAsync(id);
            if (x == null) return NotFound();
            if (x.Status != "待审批") return BadRequest(new { message = "该请假单已审批" });

            x.Status = dto.Approved ? "审批通过" : "审批拒绝";
            x.Approver = dto.Approver ?? "系统管理员";
            x.ApproveComment = dto.Comment ?? "";
            x.ApprovedAt = DateTime.Now;

            // 审批通过后，自动在考勤中标记请假
            if (dto.Approved)
            {
                for (var dt = x.StartDate.Date; dt <= x.EndDate.Date; dt = dt.AddDays(1))
                {
                    var existing = await _db.Attendances.FirstOrDefaultAsync(a => a.EmpCode == x.EmpCode && a.AttendDate.Date == dt);
                    if (existing != null)
                    {
                        existing.Status = "请假";
                        existing.Remark = $"请假：{x.LeaveType}";
                    }
                    else
                    {
                        _db.Attendances.Add(new Attendance
                        {
                            EmpCode = x.EmpCode,
                            EmpName = x.EmpName,
                            AttendDate = dt,
                            Status = "请假",
                            Remark = $"请假：{x.LeaveType}"
                        });
                    }
                }
                _db.Messages.Add(new Message
                {
                    MsgType = "审批消息",
                    Recipient = x.EmpName,
                    Content = $"您的请假申请【{x.LeaveNo}】已审批通过",
                    Creator = x.Approver
                });
            }
            else
            {
                _db.Messages.Add(new Message
                {
                    MsgType = "审批消息",
                    Recipient = x.EmpName,
                    Content = $"您的请假申请【{x.LeaveNo}】已被拒绝",
                    Creator = x.Approver
                });
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = x.Status });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.LeaveRequests.FindAsync(id);
            if (x == null) return NotFound();
            _db.LeaveRequests.Remove(x);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    public class LeaveApproveDto
    {
        public bool Approved { get; set; }
        public string? Approver { get; set; }
        public string? Comment { get; set; }
    }

    [Route("api/[controller]")]
    public class PayrollsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PayrollsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? payMonth)
        {
            var q = _db.Payrolls.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(p => p.EmpName.Contains(keyword) || p.EmpCode.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(payMonth)) q = q.Where(p => p.PayMonth == payMonth);
            var list = await q.OrderByDescending(p => p.PayMonth).ThenBy(p => p.EmpCode).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var x = await _db.Payrolls.FindAsync(id);
            if (x == null) return NotFound();
            return Ok(x);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Payroll item)
        {
            item.ActualSalary = item.BaseSalary + item.PostSalary + item.Performance + item.Overtime + item.Bonus
                - item.Deduction - item.SocialInsurance - item.HousingFund;
            _db.Payrolls.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Payroll input)
        {
            var x = await _db.Payrolls.FindAsync(id);
            if (x == null) return NotFound();
            x.EmpCode = input.EmpCode;
            x.EmpName = input.EmpName;
            x.PayMonth = input.PayMonth;
            x.BaseSalary = input.BaseSalary;
            x.PostSalary = input.PostSalary;
            x.Performance = input.Performance;
            x.Overtime = input.Overtime;
            x.Bonus = input.Bonus;
            x.Deduction = input.Deduction;
            x.SocialInsurance = input.SocialInsurance;
            x.HousingFund = input.HousingFund;
            x.ActualSalary = input.BaseSalary + input.PostSalary + input.Performance + input.Overtime + input.Bonus
                - input.Deduction - input.SocialInsurance - input.HousingFund;
            x.Status = input.Status;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.Payrolls.FindAsync(id);
            if (x == null) return NotFound();
            _db.Payrolls.Remove(x);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    [Route("api/[controller]")]
    public class TrainingsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TrainingsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var q = _db.Trainings.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(t => t.TrainNo.Contains(keyword) || t.TrainName.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(t => t.Status == status);
            var list = await q.OrderByDescending(t => t.CreatedAt).Select(t => new
            {
                t.Id, t.TrainNo, t.TrainName, t.TrainType, t.Trainer, t.TrainDate,
                t.Location, t.Status, t.Remark, t.CreatedAt,
                ParticipantCount = t.Participants.Count
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _db.Trainings.Include(x => x.Participants).FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return NotFound();
            return Ok(t);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Training item)
        {
            if (string.IsNullOrEmpty(item.TrainNo))
                item.TrainNo = "PX" + DateTime.Now.ToString("yyyyMMddHHmmss");
            _db.Trainings.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Training input)
        {
            var x = await _db.Trainings.FindAsync(id);
            if (x == null) return NotFound();
            x.TrainName = input.TrainName;
            x.TrainType = input.TrainType;
            x.Trainer = input.Trainer;
            x.TrainDate = input.TrainDate;
            x.Location = input.Location;
            x.Status = input.Status;
            x.Remark = input.Remark;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.Trainings.FindAsync(id);
            if (x == null) return NotFound();
            _db.Trainings.Remove(x);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }

        // 添加参与人
        [HttpPost("{id}/participants")]
        public async Task<IActionResult> AddParticipant(int id, TrainingParticipant p)
        {
            var t = await _db.Trainings.FindAsync(id);
            if (t == null) return NotFound();
            p.TrainingId = id;
            _db.TrainingParticipants.Add(p);
            await _db.SaveChangesAsync();
            return Ok(p);
        }

        // 删除参与人
        [HttpDelete("participants/{pid}")]
        public async Task<IActionResult> RemoveParticipant(int pid)
        {
            var p = await _db.TrainingParticipants.FindAsync(pid);
            if (p == null) return NotFound();
            _db.TrainingParticipants.Remove(p);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }

        // 更新参与人考核结果
        [HttpPut("participants/{pid}")]
        public async Task<IActionResult> UpdateParticipant(int pid, TrainingParticipant input)
        {
            var p = await _db.TrainingParticipants.FindAsync(pid);
            if (p == null) return NotFound();
            p.Result = input.Result;
            p.Remark = input.Remark;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }
    }
}
