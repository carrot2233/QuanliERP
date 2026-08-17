using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // 班次
    public class Shift
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; } = "";
        [MaxLength(10)]
        public string StartTime { get; set; } = "08:00";
        [MaxLength(10)]
        public string EndTime { get; set; } = "17:00";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 排班计划
    public class WorkSchedule
    {
        public int Id { get; set; }
        public DateTime WorkDate { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public int ShiftId { get; set; }
        public Shift? Shift { get; set; }
        [MaxLength(50)]
        public string Workshop { get; set; } = "";
        [MaxLength(200)]
        public string Task { get; set; } = "";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
