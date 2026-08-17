using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;
using QuanliERP.Api.Dtos;
using QuanliERP.Api.Services;

namespace QuanliERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        public AuthController(AppDbContext db, JwtService jwt) { _db = db; _jwt = jwt; }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Unauthorized(new { message = "用户名或密码错误" });
            if (!user.IsActive)
                return Unauthorized(new { message = "账号已被禁用" });
            return Ok(new LoginResponse
            {
                Token = _jwt.GenerateToken(user),
                Username = user.Username,
                DisplayName = user.DisplayName,
                Role = user.Role
            });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                Username = User.Identity?.Name,
                DisplayName = User.FindFirst("DisplayName")?.Value ?? "",
                Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? ""
            });
        }
    }
}
