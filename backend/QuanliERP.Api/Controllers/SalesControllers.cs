using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesOrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SalesOrdersController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var q = _db.SalesOrders.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(o => o.OrderNo.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(o => o.Status == status);
            var list = await q.OrderByDescending(o => o.Id)
                .Select(o => new
                {
                    o.Id, o.OrderNo, o.OrderDate, o.DeliveryDate, o.Status, o.Amount, o.Remark,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.Name : "",
                    Items = o.Items.Select(i => new
                    {
                        i.Id, i.ProductId, i.Qty, i.Price, i.Amount, i.DeliveredQty,
                        ProductCode = i.Product != null ? i.Product.Code : "",
                        ProductName = i.Product != null ? i.Product.Name : "",
                        ProductSpec = i.Product != null ? i.Product.Specification : ""
                    }).ToList()
                }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var o = await _db.SalesOrders
                .Include(x => x.Items).ThenInclude(i => i.Product)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return NotFound();
            return Ok(new
            {
                o.Id, o.OrderNo, o.OrderDate, o.DeliveryDate, o.Status, o.Amount, o.Remark,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.Name ?? "",
                Items = o.Items.Select(i => new
                {
                    i.Id, i.ProductId, i.Qty, i.Price, i.Amount, i.DeliveredQty,
                    ProductCode = i.Product?.Code ?? "",
                    ProductName = i.Product?.Name ?? "",
                    ProductSpec = i.Product?.Specification ?? ""
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SalesOrder order)
        {
            if (!order.Items.Any())
                return BadRequest(new { message = "订单明细不能为空" });
            foreach (var it in order.Items)
            {
                it.Amount = it.Qty * it.Price;
                order.Amount += it.Amount;
            }
            if (string.IsNullOrEmpty(order.OrderNo))
                order.OrderNo = "SO" + DateTime.Now.ToString("yyyyMMddHHmmss");
            _db.SalesOrders.Add(order);
            await _db.SaveChangesAsync();
            return Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SalesOrder input)
        {
            var o = await _db.SalesOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return NotFound();
            if (o.Status == "完成" || o.Status == "已发货")
                return BadRequest(new { message = "已完成的订单不允许修改" });

            o.CustomerId = input.CustomerId;
            o.OrderDate = input.OrderDate;
            o.DeliveryDate = input.DeliveryDate;
            o.Remark = input.Remark;

            foreach (var old in o.Items.ToList())
            {
                var match = input.Items.FirstOrDefault(x => x.Id == old.Id);
                if (match == null)
                {
                    _db.SalesOrderItems.Remove(old);
                }
                else
                {
                    old.ProductId = match.ProductId;
                    old.Qty = match.Qty;
                    old.Price = match.Price;
                    old.Amount = match.Qty * match.Price;
                }
            }
            foreach (var it in input.Items.Where(x => x.Id == 0))
            {
                it.Amount = it.Qty * it.Price;
                o.Items.Add(it);
            }
            o.Amount = o.Items.Sum(i => i.Amount);
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] string status)
        {
            var o = await _db.SalesOrders.FindAsync(id);
            if (o == null) return NotFound();
            o.Status = status;
            await _db.SaveChangesAsync();
            return Ok(new { message = "状态更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var o = await _db.SalesOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return NotFound();
            if (o.Status == "完成" || o.Status == "已发货")
                return BadRequest(new { message = "已完成的订单不允许删除" });
            _db.SalesOrderItems.RemoveRange(o.Items);
            _db.SalesOrders.Remove(o);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeliveriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DeliveriesController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword)
        {
            var q = _db.Deliveries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(d => d.DeliveryNo.Contains(keyword) || (d.Customer != null && d.Customer.Name.Contains(keyword)));
            var list = await q.OrderByDescending(d => d.Id).Select(d => new
            {
                d.Id, d.DeliveryNo, d.DeliveryDate, d.Carrier, d.PlateNo, d.Driver, d.Status, d.Remark,
                SalesOrderId = d.SalesOrderId,
                OrderNo = d.SalesOrder != null ? d.SalesOrder.OrderNo : "",
                CustomerName = d.Customer != null ? d.Customer.Name : "",
                WarehouseName = d.Warehouse != null ? d.Warehouse.Name : "",
                Items = d.Items.Select(i => new
                {
                    i.Id, i.ProductId, i.Qty, i.Price,
                    ProductName = i.Product != null ? i.Product.Name : ""
                }).ToList()
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var d = await _db.Deliveries.Include(x => x.Items).ThenInclude(i => i.Product)
                .Include(x => x.Customer).Include(x => x.Warehouse).Include(x => x.SalesOrder)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (d == null) return NotFound();
            return Ok(new
            {
                d.Id, d.DeliveryNo, d.DeliveryDate, d.Carrier, d.PlateNo, d.Driver, d.Status, d.Remark,
                d.SalesOrderId, OrderNo = d.SalesOrder?.OrderNo ?? "",
                d.CustomerId, CustomerName = d.Customer?.Name ?? "",
                d.WarehouseId, WarehouseName = d.Warehouse?.Name ?? "",
                Items = d.Items.Select(i => new
                {
                    i.Id, i.ProductId, i.Qty, i.Price,
                    ProductName = i.Product?.Name ?? "",
                    ProductSpec = i.Product?.Specification ?? ""
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Delivery delivery)
        {
            if (!delivery.Items.Any())
                return BadRequest(new { message = "发货明细不能为空" });
            if (string.IsNullOrEmpty(delivery.DeliveryNo))
                delivery.DeliveryNo = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss");

            _db.Deliveries.Add(delivery);
            await _db.SaveChangesAsync();

            // 更新订单已发数量与状态，扣减成品库存
            var so = await _db.SalesOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == delivery.SalesOrderId);
            if (so != null)
            {
                foreach (var di in delivery.Items)
                {
                    var soi = so.Items.FirstOrDefault(i => i.ProductId == di.ProductId);
                    if (soi != null) soi.DeliveredQty += di.Qty;
                    var inv = await _db.Inventories.FirstOrDefaultAsync(x =>
                        x.ItemType == "产品" && x.ItemId == di.ProductId && x.WarehouseId == delivery.WarehouseId);
                    if (inv != null)
                    {
                        if (inv.Qty < di.Qty) return BadRequest(new { message = $"产品[{di.Product?.Name}]库存不足，当前库存 {inv.Qty}" });
                        inv.Qty -= di.Qty;
                        inv.UpdatedAt = DateTime.Now;
                        _db.InventoryLedgers.Add(new InventoryLedger
                        {
                            WarehouseId = delivery.WarehouseId, ItemType = "产品", ItemId = di.ProductId,
                            ItemName = di.Product?.Name ?? "", Specification = di.Product?.Specification ?? "",
                            BillType = "销售出库", BillNo = delivery.DeliveryNo, InQty = 0, OutQty = di.Qty,
                            BalanceQty = inv.Qty, Operator = User.Identity?.Name ?? "", OperationTime = DateTime.Now,
                            Remark = "销售发货"
                        });
                    }
                }
                if (so.Items.All(i => i.DeliveredQty >= i.Qty)) so.Status = "已发货";
                else so.Status = "部分发货";
            }
            await _db.SaveChangesAsync();
            return Ok(delivery);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var d = await _db.Deliveries.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (d == null) return NotFound();
            if (d.Status == "完成") return BadRequest(new { message = "已完成的发货单不允许删除" });

            // 回滚库存与订单已发数量
            var so = await _db.SalesOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == d.SalesOrderId);
            if (so != null)
            {
                foreach (var di in d.Items)
                {
                    var soi = so.Items.FirstOrDefault(i => i.ProductId == di.ProductId);
                    if (soi != null) soi.DeliveredQty -= di.Qty;
                    var inv = await _db.Inventories.FirstOrDefaultAsync(x =>
                        x.ItemType == "产品" && x.ItemId == di.ProductId && x.WarehouseId == d.WarehouseId);
                    if (inv != null)
                    {
                        inv.Qty += di.Qty;
                        inv.UpdatedAt = DateTime.Now;
                        _db.InventoryLedgers.Add(new InventoryLedger
                        {
                            WarehouseId = d.WarehouseId, ItemType = "产品", ItemId = di.ProductId,
                            ItemName = di.Product?.Name ?? "", Specification = di.Product?.Specification ?? "",
                            BillType = "发货退回", BillNo = d.DeliveryNo, InQty = di.Qty, OutQty = 0,
                            BalanceQty = inv.Qty, Operator = User.Identity?.Name ?? "", OperationTime = DateTime.Now,
                            Remark = "删除发货单回滚"
                        });
                    }
                }
                so.Status = "确认";
            }
            _db.DeliveryItems.RemoveRange(d.Items);
            _db.Deliveries.Remove(d);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功，已回滚库存" });
        }
    }
}
