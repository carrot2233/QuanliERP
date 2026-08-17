using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // 库存（实时结存）
    public class Inventory
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        [MaxLength(30)]
        public string ItemType { get; set; } = "材料"; // 材料 / 产品
        public int ItemId { get; set; }
        [MaxLength(100)]
        public string Code { get; set; } = "";
        [MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Specification { get; set; } = "";
        [MaxLength(30)]
        public string Unit { get; set; } = "";
        public decimal Qty { get; set; }
        public decimal SafeStock { get; set; }
        [MaxLength(50)]
        public string Location { get; set; } = "";
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    // 库存流水
    public class InventoryLedger
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        [MaxLength(30)]
        public string ItemType { get; set; } = "材料";
        public int ItemId { get; set; }
        [MaxLength(100)]
        public string ItemName { get; set; } = "";
        [MaxLength(100)]
        public string Specification { get; set; } = "";
        [MaxLength(50)]
        public string BillType { get; set; } = ""; // 采购入库/车间入库/生产领用/销售出库/退件/盘盈/盘亏
        [MaxLength(50)]
        public string BillNo { get; set; } = "";
        public decimal InQty { get; set; }
        public decimal OutQty { get; set; }
        public decimal BalanceQty { get; set; }
        [MaxLength(50)]
        public string Operator { get; set; } = "";
        public DateTime OperationTime { get; set; } = DateTime.Now;
        [MaxLength(200)]
        public string Remark { get; set; } = "";
    }
}
