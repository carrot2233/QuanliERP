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
    [RequirePermission("equipment:list")]
    public class EquipmentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EquipmentsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var q = _db.Equipments.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(e => e.Code.Contains(keyword) || e.Name.Contains(keyword) || e.Model.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(e => e.Status == status);
            var list = await q.OrderBy(e => e.Code).Select(e => new
            {
                e.Id, e.Code, e.Name, e.Model, e.EquipType, e.Tonnage, e.Workshop, e.Status,
                e.Manufacturer, e.PurchaseDate, e.MaintenanceCycle, e.LastMaintainDate, e.NextMaintainDate, e.Remark,
                Overdue = e.NextMaintainDate != null && e.NextMaintainDate < DateTime.Today
            }).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Equipment item)
        {
            _db.Equipments.Add(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Equipment input)
        {
            var x = await _db.Equipments.FindAsync(id);
            if (x == null) return NotFound();
            x.Code = input.Code;
            x.Name = input.Name;
            x.Model = input.Model;
            x.EquipType = input.EquipType;
            x.Tonnage = input.Tonnage;
            x.Workshop = input.Workshop;
            x.Status = input.Status;
            x.Manufacturer = input.Manufacturer;
            x.PurchaseDate = input.PurchaseDate;
            x.MaintenanceCycle = input.MaintenanceCycle;
            x.LastMaintainDate = input.LastMaintainDate;
            x.NextMaintainDate = input.NextMaintainDate;
            x.Remark = input.Remark;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.Equipments.FindAsync(id);
            if (x == null) return NotFound();
            _db.Equipments.Remove(x);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequirePermission("equipment:maintenance")]
    public class EquipmentMaintenancesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EquipmentMaintenancesController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? equipmentId)
        {
            var q = _db.EquipmentMaintenances.AsQueryable();
            if (equipmentId.HasValue) q = q.Where(m => m.EquipmentId == equipmentId.Value);
            var list = await q.OrderByDescending(m => m.MaintainDate).Select(m => new
            {
                m.Id, m.EquipmentId, EquipmentName = m.Equipment != null ? m.Equipment.Name : "",
                m.MaintainDate, m.Type, m.Content, m.Cost, m.Handler, m.Result, m.Remark
            }).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EquipmentMaintenance item)
        {
            _db.EquipmentMaintenances.Add(item);
            var eq = await _db.Equipments.FindAsync(item.EquipmentId);
            if (eq != null)
            {
                eq.LastMaintainDate = item.MaintainDate;
                eq.Status = "运行";
            }
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EquipmentMaintenance input)
        {
            var x = await _db.EquipmentMaintenances.FindAsync(id);
            if (x == null) return NotFound();
            x.EquipmentId = input.EquipmentId;
            x.MaintainDate = input.MaintainDate;
            x.Type = input.Type;
            x.Content = input.Content;
            x.Cost = input.Cost;
            x.Handler = input.Handler;
            x.Result = input.Result;
            x.Remark = input.Remark;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.EquipmentMaintenances.FindAsync(id);
            if (x == null) return NotFound();
            _db.EquipmentMaintenances.Remove(x);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }
}
