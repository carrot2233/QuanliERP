using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Authorization;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly AppDbContext _db;
        public InventoryController(AppDbContext db) { _db = db; }

        [HttpGet]
        [RequirePermission("warehouse:inventory")]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? itemType, [FromQuery] string? warehouseId)
        {
            var q = _db.Inventories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(i => i.Code.Contains(keyword) || i.Name.Contains(keyword) || i.Specification.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(itemType))
                q = q.Where(i => i.ItemType == itemType);
            if (!string.IsNullOrWhiteSpace(warehouseId) && int.TryParse(warehouseId, out int wid))
                q = q.Where(i => i.WarehouseId == wid);
            var list = await q.OrderBy(i => i.ItemType).ThenBy(i => i.Code).Select(i => new
            {
                i.Id, i.WarehouseId, WarehouseName = i.Warehouse != null ? i.Warehouse.Name : "",
                i.ItemType, i.ItemId, i.Code, i.Name, i.Specification, i.Unit, i.Qty, i.SafeStock, i.Location,
                i.UpdatedAt,
                IsLow = i.Qty <= i.SafeStock,
                StockStatus = i.Qty <= 0 ? "缺货" : i.Qty <= i.SafeStock ? "预警" : "正常"
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("warnings")]
        [RequirePermission("warehouse:warning")]
        public async Task<IActionResult> GetWarnings()
        {
            var list = await _db.Inventories.Where(i => i.Qty <= i.SafeStock).Select(i => new
            {
                i.Id, i.WarehouseId, WarehouseName = i.Warehouse != null ? i.Warehouse.Name : "",
                i.ItemType, i.ItemId, i.Code, i.Name, i.Specification, i.Unit, i.Qty, i.SafeStock,
                StockStatus = i.Qty <= 0 ? "缺货" : "预警"
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("ledger")]
        [RequirePermission("warehouse:ledger")]
        public async Task<IActionResult> GetLedger([FromQuery] string? itemName, [FromQuery] string? billType,
            [FromQuery] string? start, [FromQuery] string? end, [FromQuery] string? direction)
        {
            var q = _db.InventoryLedgers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(itemName)) q = q.Where(l => l.ItemName.Contains(itemName));
            if (!string.IsNullOrWhiteSpace(billType)) q = q.Where(l => l.BillType == billType);
            if (DateTime.TryParse(start, out var d1)) q = q.Where(l => l.OperationTime >= d1);
            if (DateTime.TryParse(end, out var d2)) q = q.Where(l => l.OperationTime <= d2);
            if (direction == "in") q = q.Where(l => l.InQty > 0 && l.OutQty == 0);
            if (direction == "out") q = q.Where(l => l.OutQty > 0 && l.InQty == 0);
            var list = await q.OrderByDescending(l => l.OperationTime).ToListAsync();
            return Ok(list);
        }

        // 编辑出入库记录（同步调整库存结存）
        [HttpPut("ledger/{id:int}")]
        [RequirePermission("warehouse:stock-in,warehouse:stock-out")]
        public async Task<IActionResult> UpdateLedger(int id, InventoryLedger entry)
        {
            var old = await _db.InventoryLedgers.FindAsync(id);
            if (old == null) return NotFound(new { message = "记录不存在" });
            if (entry.InQty == 0 && entry.OutQty == 0)
                return BadRequest(new { message = "出入库数量不能同时为 0" });

            var inv = await _db.Inventories.FirstOrDefaultAsync(x =>
                x.ItemType == old.ItemType && x.ItemId == old.ItemId && x.WarehouseId == old.WarehouseId);
            if (inv == null) return NotFound(new { message = "未找到对应库存记录" });

            var oldIn = old.InQty;
            var oldOut = old.OutQty;
            var newIn = entry.InQty;
            var newOut = entry.OutQty;

            if (old.WarehouseId != entry.WarehouseId || old.ItemType != entry.ItemType || old.ItemId != entry.ItemId)
            {
                var target = await _db.Inventories.FirstOrDefaultAsync(x =>
                    x.ItemType == entry.ItemType && x.ItemId == entry.ItemId && x.WarehouseId == entry.WarehouseId);
                if (target == null)
                {
                    var mat = entry.ItemType == "材料" ? await _db.Materials.FindAsync(entry.ItemId) : null;
                    var prod = entry.ItemType == "产品" ? await _db.Products.FindAsync(entry.ItemId) : null;
                    target = new Inventory
                    {
                        WarehouseId = entry.WarehouseId, ItemType = entry.ItemType, ItemId = entry.ItemId,
                        Code = mat?.Code ?? prod?.Code ?? entry.ItemName, Name = entry.ItemName,
                        Specification = entry.Specification, Unit = mat?.Unit ?? prod?.Unit ?? "",
                        Qty = 0, SafeStock = 0, UpdatedAt = DateTime.Now
                    };
                    _db.Inventories.Add(target);
                }
                inv.Qty -= oldIn - oldOut;
                if (inv.Qty < 0) inv.Qty = 0;
                if (target.Id == 0) await _db.SaveChangesAsync();
                inv = target;
            }

            if (newOut > inv.Qty)
                return BadRequest(new { message = $"当前库存不足，库存为 {inv.Qty}" });
            inv.Qty += newIn - newOut;
            inv.UpdatedAt = DateTime.Now;

            old.ItemType = entry.ItemType;
            old.ItemId = entry.ItemId;
            old.ItemName = entry.ItemName;
            old.Specification = entry.Specification;
            old.BillType = entry.BillType;
            old.BillNo = entry.BillNo;
            old.InQty = newIn;
            old.OutQty = newOut;
            old.BalanceQty = inv.Qty;
            old.Remark = entry.Remark;
            if (!string.IsNullOrWhiteSpace(entry.Operator)) old.Operator = entry.Operator;

            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功", BalanceQty = inv.Qty });
        }

        // 删除出入库记录（反向冲减库存结存）
        [HttpDelete("ledger/{id:int}")]
        [RequirePermission("warehouse:stock-in,warehouse:stock-out")]
        public async Task<IActionResult> DeleteLedger(int id)
        {
            var old = await _db.InventoryLedgers.FindAsync(id);
            if (old == null) return NotFound(new { message = "记录不存在" });
            var inv = await _db.Inventories.FirstOrDefaultAsync(x =>
                x.ItemType == old.ItemType && x.ItemId == old.ItemId && x.WarehouseId == old.WarehouseId);
            if (inv != null)
            {
                inv.Qty -= old.InQty - old.OutQty;
                if (inv.Qty < 0) inv.Qty = 0;
                inv.UpdatedAt = DateTime.Now;
            }
            _db.InventoryLedgers.Remove(old);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }

        // 通用出入库操作
        [HttpPost("stock")]
        [RequirePermission("warehouse:stock-in,warehouse:stock-out")]
        public async Task<IActionResult> StockInOut(InventoryLedger entry)
        {
            if (entry.InQty == 0 && entry.OutQty == 0)
                return BadRequest(new { message = "出入库数量不能同时为 0" });
            var inv = await _db.Inventories.FirstOrDefaultAsync(x =>
                x.ItemType == entry.ItemType && x.ItemId == entry.ItemId && x.WarehouseId == entry.WarehouseId);
            if (inv == null)
            {
                var mat = entry.ItemType == "材料" ? await _db.Materials.FindAsync(entry.ItemId) : null;
                var prod = entry.ItemType == "产品" ? await _db.Products.FindAsync(entry.ItemId) : null;
                inv = new Inventory
                {
                    WarehouseId = entry.WarehouseId, ItemType = entry.ItemType, ItemId = entry.ItemId,
                    Code = mat?.Code ?? prod?.Code ?? entry.ItemName, Name = entry.ItemName,
                    Specification = entry.Specification, Unit = mat?.Unit ?? prod?.Unit ?? "",
                    Qty = 0, SafeStock = 0, UpdatedAt = DateTime.Now
                };
                _db.Inventories.Add(inv);
            }
            if (entry.OutQty > inv.Qty)
                return BadRequest(new { message = $"当前库存不足，库存为 {inv.Qty}" });
            inv.Qty += entry.InQty - entry.OutQty;
            inv.UpdatedAt = DateTime.Now;
            entry.Operator = string.IsNullOrEmpty(entry.Operator) ? User.Identity?.Name ?? "" : entry.Operator;
            entry.OperationTime = DateTime.Now;
            entry.BalanceQty = inv.Qty;
            _db.InventoryLedgers.Add(entry);
            await _db.SaveChangesAsync();
            return Ok(new { message = "操作成功", BalanceQty = inv.Qty });
        }

        // 盘点调整
        [HttpPost("adjust")]
        [RequirePermission("warehouse:stock-in,warehouse:stock-out")]
        public async Task<IActionResult> Adjust(InventoryAdjustDto dto)
        {
            var inv = await _db.Inventories.FirstOrDefaultAsync(x =>
                x.ItemType == dto.ItemType && x.ItemId == dto.ItemId && x.WarehouseId == dto.WarehouseId);
            if (inv == null) return NotFound(new { message = "未找到库存记录" });
            var diff = dto.NewQty - inv.Qty;
            if (diff == 0) return Ok(new { message = "数量无变化" });
            inv.Qty = dto.NewQty;
            inv.UpdatedAt = DateTime.Now;
            _db.InventoryLedgers.Add(new InventoryLedger
            {
                WarehouseId = inv.WarehouseId, ItemType = inv.ItemType, ItemId = inv.ItemId,
                ItemName = inv.Name, Specification = inv.Specification,
                BillType = diff > 0 ? "盘盈" : "盘亏", BillNo = dto.BillNo ?? "PD" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                InQty = diff > 0 ? diff : 0, OutQty = diff < 0 ? -diff : 0, BalanceQty = inv.Qty,
                Operator = User.Identity?.Name ?? "", OperationTime = DateTime.Now, Remark = dto.Remark ?? "盘点调整"
            });
            await _db.SaveChangesAsync();
            return Ok(new { message = "盘点成功" });
        }

        // 车间入库（生产完工入库）
        [HttpPost("workshop-in")]
        [RequirePermission("warehouse:stock-in")]
        public async Task<IActionResult> WorkshopIn(WorkshopInDto dto)
        {
            var prod = await _db.Products.FindAsync(dto.ProductId);
            if (prod == null) return NotFound(new { message = "产品不存在" });
            var inv = await _db.Inventories.FirstOrDefaultAsync(x =>
                x.ItemType == "产品" && x.ItemId == dto.ProductId && x.WarehouseId == dto.WarehouseId);
            if (inv == null)
            {
                inv = new Inventory
                {
                    WarehouseId = dto.WarehouseId, ItemType = "产品", ItemId = dto.ProductId,
                    Code = prod.Code, Name = prod.Name, Specification = prod.Specification,
                    Unit = prod.Unit, Qty = 0, SafeStock = 0, UpdatedAt = DateTime.Now
                };
                _db.Inventories.Add(inv);
            }
            inv.Qty += dto.Qty;
            inv.UpdatedAt = DateTime.Now;
            _db.InventoryLedgers.Add(new InventoryLedger
            {
                WarehouseId = dto.WarehouseId, ItemType = "产品", ItemId = dto.ProductId,
                ItemName = prod.Name, Specification = prod.Specification,
                BillType = "车间入库", BillNo = dto.BillNo ?? "CJ" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                InQty = dto.Qty, OutQty = 0, BalanceQty = inv.Qty,
                Operator = User.Identity?.Name ?? "", OperationTime = DateTime.Now,
                Remark = dto.Remark ?? $"制号 {dto.PlanNo} 完工入库"
            });
            await _db.SaveChangesAsync();
            return Ok(new { message = "入库成功", BalanceQty = inv.Qty });
        }
    }

    public class InventoryAdjustDto
    {
        public int WarehouseId { get; set; }
        public string ItemType { get; set; } = "材料";
        public int ItemId { get; set; }
        public decimal NewQty { get; set; }
        public string? BillNo { get; set; }
        public string? Remark { get; set; }
    }

    public class WorkshopInDto
    {
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public decimal Qty { get; set; }
        public string? PlanNo { get; set; }
        public string? BillNo { get; set; }
        public string? Remark { get; set; }
    }
}
