using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? keyword)
        {
            var q = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(u => u.Username.Contains(keyword) || u.DisplayName.Contains(keyword));
            var list = await q.OrderBy(u => u.Id).Select(u => new
            {
                u.Id, u.Username, u.DisplayName, u.Role, u.Phone, u.Email, u.IsActive, u.CreatedAt
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();
            return Ok(new { u.Id, u.Username, u.DisplayName, u.Role, u.Phone, u.Email, u.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (await _db.Users.AnyAsync(u => u.Username == user.Username))
                return BadRequest(new { message = "用户名已存在" });
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, User input)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();
            u.DisplayName = input.DisplayName;
            u.Role = input.Role;
            u.Phone = input.Phone;
            u.Email = input.Email;
            u.IsActive = input.IsActive;
            if (!string.IsNullOrEmpty(input.PasswordHash) && input.PasswordHash != "******")
                u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.PasswordHash);
            await _db.SaveChangesAsync();
            return Ok(new { message = "更新成功" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();
            if (u.Username == "admin") return BadRequest(new { message = "不允许删除内置管理员" });
            _db.Users.Remove(u);
            await _db.SaveChangesAsync();
            return Ok(new { message = "删除成功" });
        }
    }
}
