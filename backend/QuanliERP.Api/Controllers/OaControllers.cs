using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Controllers
{
    [Route("api/[controller]")]
    public class NoticesController : CrudBaseController<Notice>
    {
        public NoticesController(AppDbContext db) : base(db) { }
    }

    [Route("api/[controller]")]
    public class MessagesController : CrudBaseController<Message>
    {
        public MessagesController(AppDbContext db) : base(db) { }
    }

    [Route("api/[controller]")]
    public class FileRecordsController : CrudBaseController<FileRecord>
    {
        public FileRecordsController(AppDbContext db) : base(db) { }
    }

    // 流程设计
    [Route("api/[controller]")]
    public class FlowDesignsController : CrudBaseController<FlowDesign>
    {
        public FlowDesignsController(AppDbContext db) : base(db) { }

        public override async Task<IActionResult> GetAll([FromQuery] string? keyword)
        {
            var q = _db.FlowDesigns.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(f => f.FlowName.Contains(keyword) || f.FlowNo.Contains(keyword) || f.Remark.Contains(keyword));
            var list = await q.OrderBy(f => f.Sort).Select(f => new
            {
                f.Id, f.FlowNo, f.FlowName, f.Remark, f.Sort, f.Status, f.DeptName, f.CreatedAt,
                NodeCount = f.Nodes.Count
            }).ToListAsync();
            return Ok(list);
        }

        public override async Task<IActionResult> GetById(int id)
        {
            var f = await _db.FlowDesigns.Include(d => d.Nodes.OrderBy(n => n.Sort)).FirstOrDefaultAsync(d => d.Id == id);
            if (f == null) return NotFound();
            return Ok(f);
        }

        protected override void PrepareNew(FlowDesign item)
        {
            if (string.IsNullOrEmpty(item.FlowNo))
                item.FlowNo = DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }

    // 流程实例（我的流程/待办/已办）
    [Route("api/[controller]")]
    public class FlowInstancesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public FlowInstancesController(AppDbContext db) { _db = db; }

        // 我的流程：我发起的
        [HttpGet("my")]
        public async Task<IActionResult> GetMy([FromQuery] string? creator, [FromQuery] string? keyword)
        {
            var q = _db.FlowInstances.AsQueryable();
            if (!string.IsNullOrWhiteSpace(creator)) q = q.Where(f => f.Creator == creator);
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(f => f.InstanceName.Contains(keyword) || f.InstanceNo.Contains(keyword));
            var list = await q.OrderByDescending(f => f.CreatedAt).Select(f => new
            {
                f.Id, f.InstanceNo, f.InstanceName, f.FlowStatus, f.CurrentNode, f.Remark,
                f.Creator, f.CreatedAt, f.FinishedAt, f.FlowDesignId
            }).ToListAsync();
            return Ok(list);
        }

        // 待办事项：分配给我的待处理任务
        [HttpGet("todo")]
        public async Task<IActionResult> GetTodo([FromQuery] string? approver, [FromQuery] string? keyword)
        {
            var q = _db.FlowTasks.Where(t => t.Status == "待处理");
            if (!string.IsNullOrWhiteSpace(approver)) q = q.Where(t => t.Approver == approver);
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(t => t.FlowInstance!.InstanceName.Contains(keyword) || t.FlowInstance.InstanceNo.Contains(keyword));
            var list = await q.OrderByDescending(t => t.CreatedAt).Select(t => new
            {
                t.Id, t.FlowInstanceId, InstanceNo = t.FlowInstance!.InstanceNo,
                InstanceName = t.FlowInstance.InstanceName, t.NodeName, t.Approver, t.Status,
                t.CreatedAt, Creator = t.FlowInstance.Creator
            }).ToListAsync();
            return Ok(list);
        }

        // 已办事项：我已处理的任务
        [HttpGet("done")]
        public async Task<IActionResult> GetDone([FromQuery] string? approver, [FromQuery] string? keyword)
        {
            var q = _db.FlowTasks.Where(t => t.Status != "待处理");
            if (!string.IsNullOrWhiteSpace(approver)) q = q.Where(t => t.Approver == approver);
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(t => t.FlowInstance!.InstanceName.Contains(keyword) || t.FlowInstance.InstanceNo.Contains(keyword));
            var list = await q.OrderByDescending(t => t.HandledAt).Select(t => new
            {
                t.Id, t.FlowInstanceId, InstanceNo = t.FlowInstance!.InstanceNo,
                InstanceName = t.FlowInstance.InstanceName, t.NodeName, t.Approver, t.Status,
                t.Comment, t.CreatedAt, t.HandledAt, Creator = t.FlowInstance.Creator,
                FlowStatus = t.FlowInstance.FlowStatus
            }).ToListAsync();
            return Ok(list);
        }

        // 获取实例详情（含任务列表）
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var f = await _db.FlowInstances
                .Include(i => i.Tasks.OrderBy(t => t.CreatedAt))
                .Include(i => i.FlowDesign)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (f == null) return NotFound();
            return Ok(f);
        }

        // 发起流程
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFlowInstanceDto dto)
        {
            var design = await _db.FlowDesigns.Include(d => d.Nodes.OrderBy(n => n.Sort))
                .FirstOrDefaultAsync(d => d.Id == dto.FlowDesignId && d.Status == "有效");
            if (design == null) return BadRequest(new { message = "流程定义不存在或已停用" });
            if (design.Nodes.Count == 0) return BadRequest(new { message = "该流程未配置审批节点" });

            var instance = new FlowInstance
            {
                InstanceNo = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                InstanceName = dto.InstanceName ?? $"{design.FlowName} {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                FlowStatus = "审批中",
                CurrentNode = design.Nodes[0].NodeName,
                Remark = dto.Remark ?? "",
                FlowDesignId = design.Id,
                Creator = dto.Creator ?? "系统管理员"
            };
            _db.FlowInstances.Add(instance);
            await _db.SaveChangesAsync();

            // 创建第一个节点的待办任务
            var firstTask = new FlowTask
            {
                FlowInstanceId = instance.Id,
                NodeName = design.Nodes[0].NodeName,
                Approver = design.Nodes[0].Approver,
                Status = "待处理"
            };
            _db.FlowTasks.Add(firstTask);

            // 发送待办消息
            _db.Messages.Add(new Message
            {
                MsgType = "待办消息",
                Recipient = design.Nodes[0].Approver,
                Content = $"您有新的流程【{instance.InstanceName}】待处理",
                Creator = instance.Creator
            });
            await _db.SaveChangesAsync();

            return Ok(instance);
        }

        // 审批（同意/拒绝）
        [HttpPost("tasks/{taskId}/approve")]
        public async Task<IActionResult> Approve(int taskId, [FromBody] ApproveDto dto)
        {
            var task = await _db.FlowTasks.Include(t => t.FlowInstance)
                .ThenInclude(i => i!.FlowDesign).ThenInclude(d => d!.Nodes.OrderBy(n => n.Sort))
                .FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null) return NotFound();
            if (task.Status != "待处理") return BadRequest(new { message = "该任务已处理" });

            var instance = task.FlowInstance!;
            var design = instance.FlowDesign!;
            var nodes = design.Nodes.OrderBy(n => n.Sort).ToList();
            var currentIndex = nodes.FindIndex(n => n.NodeName == task.NodeName);

            task.Status = dto.Approved ? "已同意" : "已拒绝";
            task.Comment = dto.Comment ?? "";
            task.HandledAt = DateTime.Now;

            if (!dto.Approved)
            {
                // 拒绝则流程结束
                instance.FlowStatus = "审批拒绝";
                instance.CurrentNode = "结束";
                instance.FinishedAt = DateTime.Now;
                _db.Messages.Add(new Message
                {
                    MsgType = "审批消息",
                    Recipient = instance.Creator,
                    Content = $"流程【{instance.InstanceName}】已被拒绝",
                    Creator = task.Approver
                });
            }
            else if (currentIndex >= 0 && currentIndex < nodes.Count - 1)
            {
                // 流转到下一节点
                var nextNode = nodes[currentIndex + 1];
                instance.CurrentNode = nextNode.NodeName;
                _db.FlowTasks.Add(new FlowTask
                {
                    FlowInstanceId = instance.Id,
                    NodeName = nextNode.NodeName,
                    Approver = nextNode.Approver,
                    Status = "待处理"
                });
                _db.Messages.Add(new Message
                {
                    MsgType = "待办消息",
                    Recipient = nextNode.Approver,
                    Content = $"您有新的流程【{instance.InstanceName}】待处理",
                    Creator = instance.Creator
                });
            }
            else
            {
                // 最后一个节点审批通过，流程结束
                instance.FlowStatus = "审批通过";
                instance.CurrentNode = "结束";
                instance.FinishedAt = DateTime.Now;
                _db.Messages.Add(new Message
                {
                    MsgType = "审批消息",
                    Recipient = instance.Creator,
                    Content = $"流程【{instance.InstanceName}】已审批通过",
                    Creator = task.Approver
                });
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = dto.Approved ? "已同意" : "已拒绝" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.FlowInstances.FindAsync(id);
            if (x == null) return NotFound();
            _db.FlowInstances.Remove(x);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    public class CreateFlowInstanceDto
    {
        public int FlowDesignId { get; set; }
        public string? InstanceName { get; set; }
        public string? Remark { get; set; }
        public string? Creator { get; set; }
    }

    public class ApproveDto
    {
        public bool Approved { get; set; }
        public string? Comment { get; set; }
    }
}
