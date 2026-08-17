using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // 客户
    public class Customer
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Code { get; set; } = "";
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(50)]
        public string Contact { get; set; } = "";
        [MaxLength(30)]
        public string Phone { get; set; } = "";
        [MaxLength(200)]
        public string Address { get; set; } = "";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 供应商
    public class Supplier
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Code { get; set; } = "";
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(50)]
        public string Contact { get; set; } = "";
        [MaxLength(30)]
        public string Phone { get; set; } = "";
        [MaxLength(200)]
        public string Address { get; set; } = "";
        [MaxLength(50)]
        public string PaymentTerms { get; set; } = "";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 原材料（板材、刀具、量具等）
    public class Material
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Code { get; set; } = "";
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Specification { get; set; } = "";
        [MaxLength(100)]
        public string MaterialType { get; set; } = "";
        [MaxLength(30)]
        public string Unit { get; set; } = "张";
        [MaxLength(30)]
        public string Category { get; set; } = "板材";
        public decimal UnitPrice { get; set; }
        public decimal MinStock { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 产品（冲压件）
    public class Product
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Code { get; set; } = "";
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Specification { get; set; } = "";
        [MaxLength(100)]
        public string Material { get; set; } = "";
        [MaxLength(100)]
        public string CustPartNo { get; set; } = "";
        [MaxLength(100)]
        public string MoldNo { get; set; } = "";
        [MaxLength(30)]
        public string Unit { get; set; } = "件";
        public int OneOutputs { get; set; } = 1;
        [MaxLength(200)]
        public string ProcessRoute { get; set; } = "";
        public decimal StandardHours { get; set; }
        public decimal SalePrice { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 仓库
    public class Warehouse
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Code { get; set; } = "";
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Location { get; set; } = "";
        [MaxLength(50)]
        public string Manager { get; set; } = "";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 员工
    public class Employee
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Code { get; set; } = "";
        [Required, MaxLength(50)]
        public string Name { get; set; } = "";
        [MaxLength(10)]
        public string Gender { get; set; } = "";
        [MaxLength(50)]
        public string Dept { get; set; } = "";
        [MaxLength(50)]
        public string Position { get; set; } = "";
        [MaxLength(20)]
        public string Phone { get; set; } = "";
        [MaxLength(30)]
        public string Status { get; set; } = "在职";
        public DateTime HireDate { get; set; } = DateTime.Now;
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
