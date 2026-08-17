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
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PurchaseOrdersController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var q = _db.PurchaseOrders.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(o => o.OrderNo.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(o => o.Status == status);
            var list = await q.OrderByDescending(o => o.Id).Select(o => new
            {
                o.Id, o.OrderNo, o.OrderDate, o.ExpectDate, o.Status, o.Amount, o.Remark,
                SupplierId = o.SupplierId,
                SupplierName = o.Supplier != null ? o.Supplier.Name : "",
                Items = o.Items.Select(i => new
                {
                    i.Id, i.MaterialId, i.Qty, i.Price, i.Amount, i.ReceivedQty,
                    MaterialName = i.Material != null ? i.Material.Name : "",
                    MaterialSpec = i.Material != null ? i.Material.Specification : ""
                }).ToList()
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var o = await _db.PurchaseOrders.Include(x => x.Items).ThenInclude(i => i.Material)
                .Include(x => x.Supplier).FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return NotFound();
            return Ok(new
            {
                o.Id, o.OrderNo, o.OrderDate, o.ExpectDate, o.Status, o.Amount, o.Remark,
                SupplierId = o.SupplierId, SupplierName = o.Supplier?.Name ?? "",
                Items = o.Items.Select(i => new
                {
                    i.Id, i.MaterialId, i.Qty, i.Price, i.Amount, i.ReceivedQty,
                    MaterialName = i.Material?.Name ?? "",
                    MaterialSpec = i.Material?.Specification ?? ""
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(PurchaseOrder order)
        {
            if (!order.Items.Any())
                return BadRequest(new { message = "采购明细不能为空" });
            foreach (var it in order.Items)
            {
                it.Amount = it.Qty * it.Price;
                order.Amount += it.Amount;
            }
            if (string.IsNullOrEmpty(order.OrderNo))
                order.OrderNo = "PO" + DateTime.Now.ToString("yyyyMMddHHmmss");
            _db.PurchaseOrders.Add(order);
            await _db.SaveChangesAsync();
            return Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PurchaseOrder input)
        {
            var o = await _db.PurchaseOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return NotFound();
            o.SupplierId = input.SupplierId;
            o.OrderDate = input.OrderDate;
            o.ExpectDate = input.ExpectDate;
            o.Remark = input.Remark;
            foreach (var old in o.Items.ToList())
            {
                var match = input.Items.FirstOrDefault(x => x.Id == old.Id);
                if (match == null) _db.PurchaseOrderItems.Remove(old);
                else
                {
                    old.MaterialId = match.MaterialId;
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
            var o = await _db.PurchaseOrders.FindAsync(id);
            if (o == null) return NotFound();
            o.Status = status;
            await _db.SaveChangesAsync();
            return Ok(new { message = "状态更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var o = await _db.PurchaseOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return NotFound();
            if (o.Status == "已到货" || o.Status == "完成")
                return BadRequest(new { message = "已到货的订单不允许删除" });
            _db.PurchaseOrderItems.RemoveRange(o.Items);
            _db.PurchaseOrders.Remove(o);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PurchaseReceiptsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PurchaseReceiptsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword)
        {
            var q = _db.PurchaseReceipts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(r => r.ReceiptNo.Contains(keyword) || (r.Supplier != null && r.Supplier.Name.Contains(keyword)));
            var list = await q.OrderByDescending(r => r.Id).Select(r => new
            {
                r.Id, r.ReceiptNo, r.ReceiptDate, r.Status, r.Remark,
                PurchaseOrderId = r.PurchaseOrderId,
                OrderNo = r.PurchaseOrder != null ? r.PurchaseOrder.OrderNo : "",
                SupplierName = r.Supplier != null ? r.Supplier.Name : "",
                WarehouseName = r.Warehouse != null ? r.Warehouse.Name : "",
                Items = r.Items.Select(i => new
                {
                    i.Id, i.MaterialId, i.Qty, i.Price,
                    MaterialName = i.Material != null ? i.Material.Name : "",
                    MaterialSpec = i.Material != null ? i.Material.Specification : ""
                }).ToList()
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var r = await _db.PurchaseReceipts.Include(x => x.Items).ThenInclude(i => i.Material)
                .Include(x => x.Supplier).Include(x => x.Warehouse).Include(x => x.PurchaseOrder)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            return Ok(new
            {
                r.Id, r.ReceiptNo, r.ReceiptDate, r.Status, r.Remark,
                r.PurchaseOrderId, OrderNo = r.PurchaseOrder?.OrderNo ?? "",
                r.SupplierId, SupplierName = r.Supplier?.Name ?? "",
                r.WarehouseId, WarehouseName = r.Warehouse?.Name ?? "",
                Items = r.Items.Select(i => new
                {
                    i.Id, i.MaterialId, i.Qty, i.Price,
                    MaterialName = i.Material?.Name ?? "",
                    MaterialSpec = i.Material?.Specification ?? ""
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(PurchaseReceipt receipt)
        {
            if (!receipt.Items.Any())
                return BadRequest(new { message = "到货明细不能为空" });
            if (string.IsNullOrEmpty(receipt.ReceiptNo))
                receipt.ReceiptNo = "SH" + DateTime.Now.ToString("yyyyMMddHHmmss");

            _db.PurchaseReceipts.Add(receipt);
            await _db.SaveChangesAsync();

            // 更新采购订单已收数量与状态，增加材料库存
            var po = await _db.PurchaseOrders.Include(x => x.Items)
                .FirstOrDefaultAsync(x => receipt.PurchaseOrderId != null && x.Id == receipt.PurchaseOrderId);
            if (po != null)
            {
                foreach (var ri in receipt.Items)
                {
                    var poi = po.Items.FirstOrDefault(i => i.MaterialId == ri.MaterialId);
                    if (poi != null) poi.ReceivedQty += ri.Qty;
                    await StockInMaterial(receipt, ri);
                }
                po.Status = po.Items.All(i => i.ReceivedQty >= i.Qty) ? "已到货" : "部分到货";
            }
            else
            {
                foreach (var ri in receipt.Items)
                    await StockInMaterial(receipt, ri);
            }
            await _db.SaveChangesAsync();
            return Ok(receipt);
        }

        private async Task StockInMaterial(PurchaseReceipt receipt, PurchaseReceiptItem ri)
        {
            var mat = await _db.Materials.FindAsync(ri.MaterialId);
            var inv = await _db.Inventories.FirstOrDefaultAsync(x =>
                x.ItemType == "材料" && x.ItemId == ri.MaterialId && x.WarehouseId == receipt.WarehouseId);
            if (inv == null)
            {
                inv = new Inventory
                {
                    WarehouseId = receipt.WarehouseId, ItemType = "材料", ItemId = ri.MaterialId,
                    Code = mat?.Code ?? "", Name = mat?.Name ?? "", Specification = mat?.Specification ?? "",
                    Unit = mat?.Unit ?? "", Qty = 0, SafeStock = mat?.MinStock ?? 0, UpdatedAt = DateTime.Now
                };
                _db.Inventories.Add(inv);
            }
            inv.Qty += ri.Qty;
            inv.UpdatedAt = DateTime.Now;
            _db.InventoryLedgers.Add(new InventoryLedger
            {
                WarehouseId = receipt.WarehouseId, ItemType = "材料", ItemId = ri.MaterialId,
                ItemName = mat?.Name ?? "", Specification = mat?.Specification ?? "",
                BillType = "采购入库", BillNo = receipt.ReceiptNo, InQty = ri.Qty, OutQty = 0,
                BalanceQty = inv.Qty, Operator = User.Identity?.Name ?? "", OperationTime = DateTime.Now,
                Remark = "采购到货入库"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _db.PurchaseReceipts.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            var po = await _db.PurchaseOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == r.PurchaseOrderId);
            foreach (var ri in r.Items)
            {
                var poi = po?.Items.FirstOrDefault(i => i.MaterialId == ri.MaterialId);
                if (poi != null) poi.ReceivedQty -= ri.Qty;
                var inv = await _db.Inventories.FirstOrDefaultAsync(x =>
                    x.ItemType == "材料" && x.ItemId == ri.MaterialId && x.WarehouseId == r.WarehouseId);
                if (inv != null)
                {
                    inv.Qty -= ri.Qty;
                    inv.UpdatedAt = DateTime.Now;
                    _db.InventoryLedgers.Add(new InventoryLedger
                    {
                        WarehouseId = r.WarehouseId, ItemType = "材料", ItemId = ri.MaterialId,
                        ItemName = ri.Material?.Name ?? "", Specification = ri.Material?.Specification ?? "",
                        BillType = "入库冲销", BillNo = r.ReceiptNo, InQty = 0, OutQty = ri.Qty,
                        BalanceQty = inv.Qty, Operator = User.Identity?.Name ?? "", OperationTime = DateTime.Now,
                        Remark = "删除到货单回滚"
                    });
                }
            }
            _db.PurchaseReceiptItems.RemoveRange(r.Items);
            _db.PurchaseReceipts.Remove(r);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功，已回滚库存" });
        }
    }
}
