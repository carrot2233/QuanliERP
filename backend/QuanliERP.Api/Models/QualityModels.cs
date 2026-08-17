using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // 质检记录
    public class QualityInspection
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string InspectionNo { get; set; } = "";
        public DateTime InspectDate { get; set; } = DateTime.Now;
        [MaxLength(50)]
        public string PlanNo { get; set; } = "";
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        [MaxLength(50)]
        public string ProcessName { get; set; } = "";
        public decimal InspectQty { get; set; }
        public decimal QualifiedQty { get; set; }
        public decimal DefectQty { get; set; }
        [MaxLength(200)]
        public string DefectReason { get; set; } = "";
        [MaxLength(30)]
        public string Result { get; set; } = "合格"; // 合格/不合格/返工
        [MaxLength(50)]
        public string Inspector { get; set; } = "";
        [MaxLength(100)]
        public string Handler { get; set; } = "";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 计量器具台账（量具/工装）
    public class MeasuringTool
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string ToolNo { get; set; } = "";
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Specification { get; set; } = "";
        public decimal Qty { get; set; }
        [MaxLength(30)]
        public string Status { get; set; } = "在用"; // 在用/停用/封存/报废/待检
        [MaxLength(100)]
        public string Origin { get; set; } = "";
        public DateTime? PurchaseDate { get; set; }
        public decimal UnitPrice { get; set; }
        [MaxLength(50)]
        public string Dept { get; set; } = "";
        [MaxLength(50)]
        public string Holder { get; set; } = "";
        public DateTime? ReceiveDate { get; set; }
        [MaxLength(50)]
        public string CalibrationCycle { get; set; } = "";
        public DateTime? CalibrationPlanDate { get; set; }
        public DateTime? CalibrationDate { get; set; }
        public DateTime? StopDate { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 量具申购单
    public class ToolApply
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string ApplyNo { get; set; } = "";
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Specification { get; set; } = "";
        public decimal Qty { get; set; }
        [MaxLength(200)]
        public string Reason { get; set; } = "";
        [MaxLength(50)]
        public string Dept { get; set; } = "";
        public DateTime ApplyDate { get; set; } = DateTime.Now;
        public DateTime? ArrivalDate { get; set; }
        [MaxLength(30)]
        public string AuditStatus { get; set; } = "待审核"; // 待审核/同意/驳回
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 计量器具报废处理单
    public class ToolScrap
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string ScrapNo { get; set; } = "";
        [MaxLength(100)]
        public string ToolName { get; set; } = "";
        [MaxLength(100)]
        public string Specification { get; set; } = "";
        [MaxLength(50)]
        public string ManageNo { get; set; } = "";
        [MaxLength(50)]
        public string FactoryNo { get; set; } = "";
        [MaxLength(100)]
        public string Manufacturer { get; set; } = "";
        [MaxLength(50)]
        public string Holder { get; set; } = "";
        public decimal Qty { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public DateTime ScrapDate { get; set; } = DateTime.Now;
        [MaxLength(300)]
        public string Reason { get; set; } = "";
        [MaxLength(50)]
        public string Applicant { get; set; } = "";
        [MaxLength(50)]
        public string Approver { get; set; } = "";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 计量器具检定结果处理单
    public class ToolCalibration
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string CalibrationNo { get; set; } = "";
        [MaxLength(100)]
        public string ToolName { get; set; } = "";
        [MaxLength(100)]
        public string MeasureRange { get; set; } = "";
        [MaxLength(50)]
        public string ToolNo { get; set; } = "";
        [MaxLength(100)]
        public string Origin { get; set; } = "";
        public DateTime? ReceiveDate { get; set; }
        [MaxLength(50)]
        public string Dept { get; set; } = "";
        [MaxLength(50)]
        public string UserName { get; set; } = "";
        [MaxLength(30)]
        public string Result { get; set; } = "待检定"; // 待检定/合格/不合格
        [MaxLength(300)]
        public string AnomalyDesc { get; set; } = "";
        [MaxLength(300)]
        public string HandleAdvice { get; set; } = "";
        [MaxLength(300)]
        public string ReviewAdvice { get; set; } = "";
        [MaxLength(50)]
        public string Reviewer { get; set; } = "";
        public DateTime? ReviewDate { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
