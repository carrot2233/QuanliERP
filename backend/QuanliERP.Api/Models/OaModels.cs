using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // ========== 协同办公 ==========

    // 通知公告
    public class Notice
    {
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Title { get; set; } = "";
        [MaxLength(2000)]
        public string Content { get; set; } = "";
        [MaxLength(30)]
        public string Status { get; set; } = "有效"; // 有效/无效
        [MaxLength(50)]
        public string Creator { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 消息中心
    public class Message
    {
        public int Id { get; set; }
        [MaxLength(30)]
        public string MsgType { get; set; } = "系统消息"; // 系统消息/审批消息/待办消息
        [MaxLength(50)]
        public string Recipient { get; set; } = ""; // 收件人
        [MaxLength(500)]
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(50)]
        public string Creator { get; set; } = ""; // 创建用户
    }

    // 流程设计
    public class FlowDesign
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string FlowNo { get; set; } = ""; // 流程编号
        [Required, MaxLength(100)]
        public string FlowName { get; set; } = ""; // 流程名称
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public int Sort { get; set; } = 1;
        [MaxLength(30)]
        public string Status { get; set; } = "有效"; // 有效/无效
        [MaxLength(50)]
        public string DeptName { get; set; } = ""; // 所属部门
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<FlowNode> Nodes { get; set; } = new();
    }

    // 流程节点
    public class FlowNode
    {
        public int Id { get; set; }
        public int FlowDesignId { get; set; }
        public FlowDesign? FlowDesign { get; set; }
        [MaxLength(50)]
        public string NodeName { get; set; } = ""; // 节点名称
        [MaxLength(50)]
        public string Approver { get; set; } = ""; // 审批人
        public int Sort { get; set; } = 1; // 节点顺序
    }

    // 流程实例（我的流程/待办/已办）
    public class FlowInstance
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string InstanceNo { get; set; } = ""; // 实例编号
        [MaxLength(200)]
        public string InstanceName { get; set; } = ""; // 实例名称
        [MaxLength(30)]
        public string FlowStatus { get; set; } = "审批中"; // 审批中/审批通过/审批拒绝
        [MaxLength(100)]
        public string CurrentNode { get; set; } = ""; // 当前节点名称
        [MaxLength(200)]
        public string Remark { get; set; } = ""; // 实例备注
        public int FlowDesignId { get; set; }
        public FlowDesign? FlowDesign { get; set; }
        [MaxLength(50)]
        public string Creator { get; set; } = ""; // 创建人
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? FinishedAt { get; set; }

        public List<FlowTask> Tasks { get; set; } = new();
    }

    // 流程任务（待办/已办）
    public class FlowTask
    {
        public int Id { get; set; }
        public int FlowInstanceId { get; set; }
        public FlowInstance? FlowInstance { get; set; }
        [MaxLength(100)]
        public string NodeName { get; set; } = ""; // 节点名称
        [MaxLength(50)]
        public string Approver { get; set; } = ""; // 审批人
        [MaxLength(30)]
        public string Status { get; set; } = "待处理"; // 待处理/已同意/已拒绝
        [MaxLength(500)]
        public string Comment { get; set; } = ""; // 审批意见
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? HandledAt { get; set; }
    }

    // 文件管理
    public class FileRecord
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string FileName { get; set; } = ""; // 文件名称
        [MaxLength(50)]
        public string FileType { get; set; } = ""; // 文件类型（图片/文档/表格...）
        [MaxLength(100)]
        public string Category { get; set; } = ""; // 文件所属
        [MaxLength(100)]
        public string DeptName { get; set; } = ""; // 所属部门
        [MaxLength(30)]
        public string Status { get; set; } = "有效";
        [MaxLength(50)]
        public string Creator { get; set; } = "";
        [MaxLength(200)]
        public string Remark { get; set; } = ""; // 文件备注
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
