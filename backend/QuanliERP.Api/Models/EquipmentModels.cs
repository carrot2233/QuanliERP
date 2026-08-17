using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // 设备（冲床/油压机/剪板机/车床/线切割/钻床等）
    public class Equipment
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Code { get; set; } = "";
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Model { get; set; } = "";
        [MaxLength(50)]
        public string EquipType { get; set; } = "";
        public decimal Tonnage { get; set; }
        [MaxLength(50)]
        public string Workshop { get; set; } = "";
        [MaxLength(30)]
        public string Status { get; set; } = "运行"; // 运行/维修/停机/报废
        [MaxLength(100)]
        public string Manufacturer { get; set; } = "";
        public DateTime? PurchaseDate { get; set; }
        [MaxLength(50)]
        public string MaintenanceCycle { get; set; } = "";
        public DateTime? LastMaintainDate { get; set; }
        public DateTime? NextMaintainDate { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 设备维护保养/维修记录
    public class EquipmentMaintenance
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }
        public DateTime MaintainDate { get; set; } = DateTime.Now;
        [MaxLength(30)]
        public string Type { get; set; } = "保养"; // 保养/维修/点检
        [MaxLength(300)]
        public string Content { get; set; } = "";
        public decimal Cost { get; set; }
        [MaxLength(50)]
        public string Handler { get; set; } = "";
        [MaxLength(30)]
        public string Result { get; set; } = "";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
