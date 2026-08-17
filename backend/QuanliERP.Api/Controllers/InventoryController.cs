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
    public class InventoryController : ControllerBase
    {
        private readonly AppDbContext _db;
        public InventoryController(AppDbContext db) { _db = db; }

        [HttpGet]
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
        public async Task<IActionResult> GetLedger([FromQuery] string? itemName, [FromQuery] string? billType,
            [FromQuery] string? start, [FromQuery] string? end)
        {
            var q = _db.InventoryLedgers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(itemName)) q = q.Where(l => l.ItemName.Contains(itemName));
            if (!string.IsNullOrWhiteSpace(billType)) q = q.Where(l => l.BillType == billType);
            if (DateTime.TryParse(start, out var d1)) q = q.Where(l => l.OperationTime >= d1);
            if (DateTime.TryParse(end, out var d2)) q = q.Where(l => l.OperationTime <= d2);
            var list = await q.OrderByDescending(l => l.OperationTime).ToListAsync();
            return Ok(list);
        }

        // 通用出入库操作
        [HttpPost("stock")]
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
