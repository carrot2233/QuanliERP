using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // 销售订单
    public class SalesOrder
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string OrderNo { get; set; } = "";
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime DeliveryDate { get; set; }
        [MaxLength(30)]
        public string Status { get; set; } = "草稿";
        public decimal Amount { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<SalesOrderItem> Items { get; set; } = new();
    }

    public class SalesOrderItem
    {
        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal DeliveredQty { get; set; }
    }

    // 销售发货单
    public class Delivery
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string DeliveryNo { get; set; } = "";
        public int SalesOrderId { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public DateTime DeliveryDate { get; set; } = DateTime.Now;
        [MaxLength(50)]
        public string Carrier { get; set; } = "";
        [MaxLength(30)]
        public string PlateNo { get; set; } = "";
        [MaxLength(50)]
        public string Driver { get; set; } = "";
        [MaxLength(30)]
        public string Status { get; set; } = "已发货";
        [MaxLength(200)]
        public string Remark { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<DeliveryItem> Items { get; set; } = new();
    }

    public class DeliveryItem
    {
        public int Id { get; set; }
        public int DeliveryId { get; set; }
        public Delivery? Delivery { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }
}
