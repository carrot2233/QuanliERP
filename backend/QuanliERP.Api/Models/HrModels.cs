using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // ========== 人力资源 ==========

    // 考勤记录
    public class Attendance
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string EmpCode { get; set; } = ""; // 工号
        [MaxLength(50)]
        public string EmpName { get; set; } = ""; // 姓名
        public DateTime AttendDate { get; set; } = DateTime.Now; // 考勤日期
        [MaxLength(10)]
        public string CheckIn { get; set; } = ""; // 上班打卡
        [MaxLength(10)]
        public string CheckOut { get; set; } = ""; // 下班打卡
        [MaxLength(30)]
        public string Status { get; set; } = "正常"; // 正常/迟到/早退/缺勤/请假
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 请假单
    public class LeaveRequest
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string LeaveNo { get; set; } = ""; // 请假单号
        [MaxLength(50)]
        public string EmpCode { get; set; } = "";
        [MaxLength(50)]
        public string EmpName { get; set; } = "";
        [MaxLength(30)]
        public string LeaveType { get; set; } = "事假"; // 事假/病假/年假/调休/婚假/产假
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Days { get; set; } // 请假天数
        [MaxLength(500)]
        public string Reason { get; set; } = "";
        [MaxLength(30)]
        public string Status { get; set; } = "待审批"; // 待审批/审批通过/审批拒绝
        [MaxLength(50)]
        public string Approver { get; set; } = ""; // 审批人
        [MaxLength(500)]
        public string ApproveComment { get; set; } = ""; // 审批意见
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 薪资单
    public class Payroll
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string EmpCode { get; set; } = "";
        [MaxLength(50)]
        public string EmpName { get; set; } = "";
        [MaxLength(20)]
        public string PayMonth { get; set; } = ""; // 薪资月份 2026-08
        public decimal BaseSalary { get; set; } // 基本工资
        public decimal PostSalary { get; set; } // 岗位工资
        public decimal Performance { get; set; } // 绩效工资
        public decimal Overtime { get; set; } // 加班费
        public decimal Bonus { get; set; } // 奖金
        public decimal Deduction { get; set; } // 扣款
        public decimal SocialInsurance { get; set; } // 社保
        public decimal HousingFund { get; set; } // 公积金
        public decimal ActualSalary { get; set; } // 实发工资
        [MaxLength(30)]
        public string Status { get; set; } = "待发放"; // 待发放/已发放
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 培训记录
    public class Training
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string TrainNo { get; set; } = ""; // 培训编号
        [Required, MaxLength(100)]
        public string TrainName { get; set; } = ""; // 培训名称
        [MaxLength(50)]
        public string TrainType { get; set; } = "内部培训"; // 内部培训/外部培训
        [MaxLength(50)]
        public string Trainer { get; set; } = ""; // 培训讲师
        public DateTime? TrainDate { get; set; } // 培训日期
        [MaxLength(100)]
        public string Location { get; set; } = ""; // 培训地点
        [MaxLength(30)]
        public string Status { get; set; } = "计划中"; // 计划中/进行中/已完成
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<TrainingParticipant> Participants { get; set; } = new();
    }

    // 培训参与人
    public class TrainingParticipant
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public Training? Training { get; set; }
        [MaxLength(50)]
        public string EmpCode { get; set; } = "";
        [MaxLength(50)]
        public string EmpName { get; set; } = "";
        [MaxLength(30)]
        public string Result { get; set; } = "待考核"; // 待考核/合格/不合格
        [MaxLength(200)]
        public string Remark { get; set; } = "";
    }
}
