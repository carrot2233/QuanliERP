using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Authorization;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;
using QuanliERP.Api.Services;

namespace QuanliERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    [RequirePermission("system:role")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RolesController(AppDbContext db) { _db = db; }

        // 角色列表（含权限码）
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _db.Roles.OrderBy(r => r.Id).ToListAsync();
            var perms = await _db.RolePermissions.ToListAsync();
            var list = roles.Select(r => new
            {
                r.Id, r.Code, r.Name, r.Description, r.IsBuiltIn, r.CreatedAt,
                Permissions = perms.Where(p => p.RoleId == r.Id).Select(p => p.PermissionCode).ToList()
            }).ToList();
            return Ok(list);
        }

        // 权限目录树（菜单）
        [HttpGet("menus")]
        public IActionResult GetMenus() => Ok(MenuCatalog.Menus);

        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            if (await _db.Roles.AnyAsync(r => r.Code == role.Code))
                return BadRequest(new { message = "角色编码已存在" });
            role.IsBuiltIn = false;
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return Ok(new { message = "新增成功", Id = role.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Role input)
        {
            var r = await _db.Roles.FindAsync(id);
            if (r == null) return NotFound();
            r.Name = input.Name;
            r.Description = input.Description;
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _db.Roles.FindAsync(id);
            if (r == null) return NotFound();
            if (r.IsBuiltIn) return BadRequest(new { message = "内置角色不允许删除" });
            if (await _db.Users.AnyAsync(u => u.Role == r.Code))
                return BadRequest(new { message = "该角色下仍有用户，无法删除" });
            _db.Roles.Remove(r);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }

        // 设置角色菜单权限
        [HttpPut("{id}/permissions")]
        public async Task<IActionResult> SetPermissions(int id, [FromBody] PermissionsDto dto)
        {
            var r = await _db.Roles.FindAsync(id);
            if (r == null) return NotFound();
            if (r.IsBuiltIn && r.Code == "admin")
                return BadRequest(new { message = "管理员角色拥有全部权限，无需设置" });
            var existing = await _db.RolePermissions.Where(p => p.RoleId == id).ToListAsync();
            _db.RolePermissions.RemoveRange(existing);
            var codes = dto.Permissions ?? new List<string>();
            foreach (var c in codes.Distinct(StringComparer.OrdinalIgnoreCase))
                _db.RolePermissions.Add(new RolePermission { RoleId = id, PermissionCode = c });
            await _db.SaveChangesAsync();
            return Ok(new { message = "权限已保存" });
        }
    }

    public class PermissionsDto
    {
        public List<string>? Permissions { get; set; }
    }
}
