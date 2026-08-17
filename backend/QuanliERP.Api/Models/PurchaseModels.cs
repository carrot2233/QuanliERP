using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // 采购订单
    public class PurchaseOrder
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string OrderNo { get; set; } = "";
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime ExpectDate { get; set; }
        [MaxLength(30)]
        public string Status { get; set; } = "草稿";
        public decimal Amount { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<PurchaseOrderItem> Items { get; set; } = new();
    }

    public class PurchaseOrderItem
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        public int MaterialId { get; set; }
        public Material? Material { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal ReceivedQty { get; set; }
    }

    // 采购到货单
    public class PurchaseReceipt
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string ReceiptNo { get; set; } = "";
        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.Now;
        [MaxLength(30)]
        public string Status { get; set; } = "已入库";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<PurchaseReceiptItem> Items { get; set; } = new();
    }

    public class PurchaseReceiptItem
    {
        public int Id { get; set; }
        public int PurchaseReceiptId { get; set; }
        public PurchaseReceipt? PurchaseReceipt { get; set; }
        public int MaterialId { get; set; }
        public Material? Material { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public int? PurchaseOrderItemId { get; set; }
    }
}
