using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;

namespace QuanliERP.Api.Authorization
{
    // 权限码（菜单级），可放 Controller 或 Action 上，如 [RequirePermission("production:plan")]。
    // 支持逗号分隔多个权限码（任一命中即放行）：[RequirePermission("warehouse:stock-in,warehouse:stock-out")]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string codes) : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { codes };
        }
    }

    public class RequirePermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string[] _codes;
        private readonly AppDbContext _db;
        public RequirePermissionFilter(string codes, AppDbContext db)
        {
            _codes = codes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            _db = db;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (role == "admin") return;

            var has = await _db.RolePermissions
                .AnyAsync(rp => rp.Role != null && rp.Role.Code == role && _codes.Contains(rp.PermissionCode));
            if (!has) context.Result = new ForbidResult();
        }
    }
}
