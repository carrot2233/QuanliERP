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
        private readonly CaptchaService _captcha;
        public AuthController(AppDbContext db, JwtService jwt, CaptchaService captcha) { _db = db; _jwt = jwt; _captcha = captcha; }

        [HttpGet("captcha")]
        [AllowAnonymous]
        public IActionResult GetCaptcha()
        {
            var (key, image) = _captcha.Generate();
            return Ok(new { key, image });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            if (!_captcha.Validate(req.CaptchaKey, req.CaptchaCode))
                return Unauthorized(new { message = "验证码错误" });

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
