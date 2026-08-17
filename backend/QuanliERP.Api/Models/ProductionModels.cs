using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // 生产计划（按制号）
    public class ProductionPlan
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string PlanNo { get; set; } = ""; // 制号 M01/M06...
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        [MaxLength(100)]
        public string ProjectName { get; set; } = "";
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public int? MaterialId { get; set; }
        public Material? Material { get; set; }
        public int OneOutputs { get; set; } = 1;
        public decimal PlanQty { get; set; }
        public DateTime PlannedStart { get; set; }
        public DateTime PlannedEnd { get; set; }
        public DateTime? ActualEnd { get; set; }
        [MaxLength(30)]
        public string Status { get; set; } = "未开始"; // 未开始/进行中/已完成/暂停
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 冲压产量单（按工序）
    public class ProductionOrder
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        [MaxLength(50)]
        public string PlanNo { get; set; } = ""; // 制号
        [MaxLength(50)]
        public string ProcessName { get; set; } = ""; // 工序名称
        [MaxLength(100)]
        public string Project { get; set; } = "";
        [MaxLength(100)]
        public string ProcessDesc { get; set; } = "";
        public decimal FinishedQty { get; set; } // 成品数量
        public decimal ScrapQty { get; set; } // 废品数量
        public decimal CompletedQty { get; set; } // 完成数量
        [MaxLength(50)]
        public string OrderNo { get; set; } = ""; // 编号
        public decimal WorkHours { get; set; } // 工时
        [MaxLength(50)]
        public string MachineNo { get; set; } = ""; // 机床
        [MaxLength(100)]
        public string Operator1 { get; set; } = "";
        [MaxLength(100)]
        public string Operator2 { get; set; } = "";
        [MaxLength(100)]
        public string Operator3 { get; set; } = "";
        [MaxLength(100)]
        public string Operator4 { get; set; } = "";
        public int? ShiftId { get; set; }
        [MaxLength(50)]
        public string ShiftName { get; set; } = "";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 生产日报表
    public class ProductionDailyReport
    {
        public int Id { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.Now;
        [Required, MaxLength(50)]
        public string PlanNo { get; set; } = "";
        public decimal PrevCarryQty { get; set; } // 上期结转
        public decimal MaterialQty { get; set; } // 领用
        [MaxLength(50)]
        public string BatchNo { get; set; } = ""; // 批号
        public decimal ScrapSheets { get; set; } // 废料张数
        public decimal InStockQty { get; set; } // 入库数量
        public decimal ShipQty { get; set; } // 发货数量
        [MaxLength(100)]
        public string SizeSpec { get; set; } = ""; // 尺寸
        [MaxLength(50)]
        public string MaterialSpec { get; set; } = ""; // 材质
        public decimal TaiFen { get; set; } // 台份
        public decimal TotalLingliao { get; set; }
        public decimal TotalFeiliao { get; set; }
        public decimal TotalChengpin { get; set; }
        public decimal TotalFeipin { get; set; }
        public decimal TotalGongshi { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<DailyReportProcess> Processes { get; set; } = new();
    }

    // 日报工序明细（落料/拉延/修边/冲孔侧冲...）
    public class DailyReportProcess
    {
        public int Id { get; set; }
        public int ProductionDailyReportId { get; set; }
        public ProductionDailyReport? ProductionDailyReport { get; set; }
        [MaxLength(50)]
        public string ProcessName { get; set; } = "";
        [MaxLength(50)]
        public string EquipmentNo { get; set; } = "";
        public decimal QualifiedQty { get; set; }
        public decimal ScrapQty { get; set; }
        public decimal WorkHours { get; set; }
    }
}
