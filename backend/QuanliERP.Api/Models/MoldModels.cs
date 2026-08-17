using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // 模具台账
    public class Mold
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string MoldNo { get; set; } = "";
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        [MaxLength(100)]
        public string ProjectName { get; set; } = "";
        [MaxLength(50)]
        public string PlanNo { get; set; } = "";
        [MaxLength(50)]
        public string ProcessType { get; set; } = "";
        public decimal Tonnage { get; set; }
        [MaxLength(30)]
        public string Status { get; set; } = "制造中"; // 设计/制造中/试模/调试完成/量产/维修/报废
        [MaxLength(100)]
        public string Location { get; set; } = "";
        [MaxLength(100)]
        public string Manager { get; set; } = "";
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 模具制造生产总计划
    public class MoldPlan
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string PlanNo { get; set; } = "";
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        [MaxLength(100)]
        public string ProjectName { get; set; } = "";
        [MaxLength(50)]
        public string MoldNo { get; set; } = "";
        [MaxLength(100)]
        public string MoldName { get; set; } = "";
        [MaxLength(50)]
        public string ProcessName { get; set; } = "";
        public decimal Tonnage { get; set; }
        [MaxLength(30)]
        public string MoldStatus { get; set; } = "排产中";
        public DateTime? PlanArrival { get; set; } // 计划到货时间
        public DateTime? ActualArrival { get; set; } // 实际到货时间
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<MoldPlanStage> Stages { get; set; } = new();
    }

    // 模具制造阶段（编程2D/编程3D/2D加工/淬火计划/投线/线切割/机钳装配/3D精加工/合模装配/研合完成/调试完成）
    public class MoldPlanStage
    {
        public int Id { get; set; }
        public int MoldPlanId { get; set; }
        public MoldPlan? MoldPlan { get; set; }
        [Required, MaxLength(50)]
        public string StageName { get; set; } = "";
        public DateTime? PlanStart { get; set; }
        public DateTime? PlanEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        [MaxLength(30)]
        public string Status { get; set; } = "未开始"; // 未开始/进行中/已完成/超期
        [MaxLength(200)]
        public string Remark { get; set; } = "";
    }
}
