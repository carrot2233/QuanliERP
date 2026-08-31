using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Authorization;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Controllers
{
    [Route("api/[controller]")]
    [RequirePermission("oa:notice")]
    public class NoticesController : CrudBaseController<Notice>
    {
        public NoticesController(AppDbContext db) : base(db) { }

        // 新增通知时同步生成消息中心待办消息（发给所有启用用户）
        public override async Task<IActionResult> Create(Notice item)
        {
            PrepareNew(item);
            _db.Notices.Add(item);
            await _db.SaveChangesAsync();

            var users = await _db.Users.Where(u => u.IsActive).Select(u => u.DisplayName).ToListAsync();
            var msgContent = item.Title + (string.IsNullOrEmpty(item.Content) ? "" : "：" + item.Content);
            if (msgContent.Length > 480) msgContent = msgContent[..480] + "...";
            foreach (var name in users)
            {
                _db.Messages.Add(new Message
                {
                    MsgType = "系统消息",
                    Recipient = name,
                    Content = msgContent,
                    Creator = string.IsNullOrEmpty(item.Creator) ? "系统管理员" : item.Creator
                });
            }
            await _db.SaveChangesAsync();
            return Ok(item);
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MessagesController(AppDbContext db) { _db = db; }

        // 收件箱：当前用户的全部消息（按置顶/时间排序）
        [HttpGet]
        [RequirePermission("oa:message")]
        public async Task<IActionResult> GetAll([FromQuery] string? recipient, [FromQuery] string? keyword, [FromQuery] string? filter)
        {
            var q = _db.Messages.AsQueryable();
            if (!string.IsNullOrWhiteSpace(recipient)) q = q.Where(m => m.Recipient == recipient);
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(m => m.Content.Contains(keyword) || m.MsgType.Contains(keyword));
            if (filter == "starred") q = q.Where(m => m.IsStarred);
            else if (filter == "read") q = q.Where(m => m.IsRead);
            else if (filter == "unread") q = q.Where(m => !m.IsRead);
            var list = await q.OrderByDescending(m => m.IsPinned).ThenByDescending(m => m.CreatedAt)
                .Select(m => new { m.Id, m.MsgType, m.Recipient, m.Content, m.Creator, m.CreatedAt, m.IsRead, m.IsStarred, m.IsPinned })
                .ToListAsync();
            return Ok(list);
        }

        // 未读消息数
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount([FromQuery] string recipient)
        {
            var count = await _db.Messages.CountAsync(m => m.Recipient == recipient && !m.IsRead);
            return Ok(new { count });
        }

        // 未读消息列表（顶部气泡下拉用）
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread([FromQuery] string recipient, [FromQuery] int limit = 10)
        {
            var list = await _db.Messages.Where(m => m.Recipient == recipient && !m.IsRead)
                .OrderByDescending(m => m.CreatedAt).Take(limit)
                .Select(m => new { m.Id, m.MsgType, m.Content, m.Creator, m.CreatedAt })
                .ToListAsync();
            return Ok(list);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var item = await _db.Messages.FindAsync(id);
            if (item == null) return NotFound();
            item.IsRead = true;
            await _db.SaveChangesAsync();
            return Ok(new { message = "已标记为已读" });
        }

        [HttpPut("{id}/star")]
        [RequirePermission("oa:message")]
        public async Task<IActionResult> ToggleStar(int id)
        {
            var item = await _db.Messages.FindAsync(id);
            if (item == null) return NotFound();
            item.IsStarred = !item.IsStarred;
            await _db.SaveChangesAsync();
            return Ok(new { message = item.IsStarred ? "已星标" : "已取消星标" });
        }

        [HttpPut("{id}/pin")]
        [RequirePermission("oa:message")]
        public async Task<IActionResult> TogglePin(int id)
        {
            var item = await _db.Messages.FindAsync(id);
            if (item == null) return NotFound();
            item.IsPinned = !item.IsPinned;
            await _db.SaveChangesAsync();
            return Ok(new { message = item.IsPinned ? "已置顶" : "已取消置顶" });
        }

        [HttpDelete("{id}")]
        [RequirePermission("oa:message")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Messages.FindAsync(id);
            if (item == null) return NotFound();
            _db.Messages.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequirePermission("oa:file")]
    public class FileRecordsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public FileRecordsController(AppDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

        private string UploadsDir => Path.Combine(_env.ContentRootPath, "uploads");

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword)
        {
            var q = _db.FileRecords.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(f => f.FileName.Contains(keyword) || f.Category.Contains(keyword) || f.DeptName.Contains(keyword) || f.Remark.Contains(keyword));
            var list = await q.OrderByDescending(f => f.CreatedAt).Select(f => new
            {
                f.Id, f.FileName, f.FileType, f.Category, f.DeptName, f.Status, f.Creator, f.Remark,
                HasAttachment = f.FilePath != ""
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _db.FileRecords.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] IFormFile? attachment, [FromForm] string? fileName, [FromForm] string? fileType,
            [FromForm] string? category, [FromForm] string? deptName, [FromForm] string? status, [FromForm] string? creator, [FromForm] string? remark)
        {
            var item = new FileRecord
            {
                FileName = string.IsNullOrWhiteSpace(fileName) ? (attachment?.FileName ?? "") : fileName,
                FileType = fileType ?? "",
                Category = category ?? "",
                DeptName = deptName ?? "",
                Status = string.IsNullOrWhiteSpace(status) ? "有效" : status,
                Creator = creator ?? "",
                Remark = remark ?? ""
            };
            if (attachment != null && attachment.Length > 0)
                item.FilePath = await SaveFile(attachment);
            _db.FileRecords.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] IFormFile? attachment, [FromForm] string? fileName, [FromForm] string? fileType,
            [FromForm] string? category, [FromForm] string? deptName, [FromForm] string? status, [FromForm] string? creator, [FromForm] string? remark,
            [FromForm] string? existingFilePath, [FromForm] bool? removeAttachment)
        {
            var item = await _db.FileRecords.FindAsync(id);
            if (item == null) return NotFound();

            item.FileName = string.IsNullOrWhiteSpace(fileName) ? item.FileName : fileName;
            item.FileType = fileType ?? item.FileType;
            item.Category = category ?? item.Category;
            item.DeptName = deptName ?? item.DeptName;
            item.Status = string.IsNullOrWhiteSpace(status) ? item.Status : status;
            item.Creator = creator ?? item.Creator;
            item.Remark = remark ?? item.Remark;

            if (removeAttachment == true)
                DeleteFile(item.FilePath);
            if (attachment != null && attachment.Length > 0)
            {
                if (item.FilePath != "" && removeAttachment != true)
                    DeleteFile(item.FilePath);
                item.FilePath = await SaveFile(attachment);
            }
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(int id)
        {
            var item = await _db.FileRecords.FindAsync(id);
            if (item == null) return NotFound();
            if (string.IsNullOrEmpty(item.FilePath))
                return NotFound(new { message = "该记录未上传附件" });
            var path = Path.Combine(_env.ContentRootPath, item.FilePath);
            if (!System.IO.File.Exists(path))
                return NotFound(new { message = "附件文件不存在" });
            var bytes = await System.IO.File.ReadAllBytesAsync(path);
            var ext = Path.GetExtension(item.FilePath);
            return File(bytes, "application/octet-stream", $"{item.FileName}{ext}");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.FileRecords.FindAsync(id);
            if (item == null) return NotFound();
            DeleteFile(item.FilePath);
            _db.FileRecords.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var dir = UploadsDir;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var ext = Path.GetExtension(file.FileName);
            var name = $"{DateTime.Now:yyyyMMddHHmmssfff}{Guid.NewGuid():N}{ext}";
            var full = Path.Combine(dir, name);
            using var stream = new FileStream(full, FileMode.Create);
            await file.CopyToAsync(stream);
            return Path.Combine("uploads", name);
        }

        private void DeleteFile(string rel)
        {
            if (string.IsNullOrEmpty(rel)) return;
            var full = Path.Combine(_env.ContentRootPath, rel);
            if (System.IO.File.Exists(full))
                System.IO.File.Delete(full);
        }
    }

    // 流程设计：读取（列表/详情）供发起流程使用，写操作仅限流程设计权限
    [Route("api/[controller]")]
    [RequirePermission("oa:flowdesign,oa:myflow,oa:todo,oa:done")]
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
                f.Id, f.FlowNo, f.FlowName, f.Remark, f.Sort, f.Status, f.DeptName, f.FormType, f.FormFields, f.CreatedAt,
                NodeCount = f.Nodes.Count
            }).ToListAsync();
            return Ok(list);
        }

        [RequirePermission("oa:flowdesign")]
        public override async Task<IActionResult> GetById(int id)
        {
            var f = await _db.FlowDesigns.Include(d => d.Nodes.OrderBy(n => n.Sort)).FirstOrDefaultAsync(d => d.Id == id);
            if (f == null) return NotFound();
            return Ok(f);
        }

        [RequirePermission("oa:flowdesign")]
        public override async Task<IActionResult> Create(FlowDesign item)
        {
            PrepareNew(item);
            // 清除导航属性，只保留节点数据
            var nodes = item.Nodes.Select((n, i) => new FlowNode { NodeName = n.NodeName, Approver = n.Approver, Sort = i + 1 }).ToList();
            item.Nodes = nodes;
            _db.FlowDesigns.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [RequirePermission("oa:flowdesign")]
        public override async Task<IActionResult> Update(int id, FlowDesign item)
        {
            var existing = await _db.FlowDesigns.Include(d => d.Nodes).FirstOrDefaultAsync(d => d.Id == id);
            if (existing == null) return NotFound();

            existing.FlowNo = item.FlowNo;
            existing.FlowName = item.FlowName;
            existing.Remark = item.Remark;
            existing.Sort = item.Sort;
            existing.Status = item.Status;
            existing.DeptName = item.DeptName;
            existing.FormType = item.FormType;
            existing.FormFields = item.FormFields;

            // 重建节点
            _db.FlowNodes.RemoveRange(existing.Nodes);
            existing.Nodes = item.Nodes.Select((n, i) => new FlowNode { NodeName = n.NodeName, Approver = n.Approver, Sort = i + 1 }).ToList();

            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [RequirePermission("oa:flowdesign")]
        public override async Task<IActionResult> Delete(int id)
        {
            var item = await _db.FlowDesigns.FindAsync(id);
            if (item == null) return NotFound();
            _db.FlowDesigns.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }

        protected override void PrepareNew(FlowDesign item)
        {
            if (string.IsNullOrEmpty(item.FlowNo))
                item.FlowNo = DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }

    // 流程实例（我的流程/待办/已办）
    [Route("api/[controller]")]
    [RequirePermission("oa:myflow,oa:todo,oa:done")]
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
                f.FormData, f.Creator, f.CreatedAt, f.FinishedAt, f.FlowDesignId
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
                FormData = dto.FormData ?? "",
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
        public string? FormData { get; set; }
        public string? Creator { get; set; }
    }

    public class ApproveDto
    {
        public bool Approved { get; set; }
        public string? Comment { get; set; }
    }
}
