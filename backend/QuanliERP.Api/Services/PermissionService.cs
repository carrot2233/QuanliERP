using Microsoft.EntityFrameworkCore;
using QuanliERP.Api.Data;
using QuanliERP.Api.Models;

namespace QuanliERP.Api.Services
{
    // 根据用户角色解析其拥有的权限码列表
    public class PermissionService
    {
        private readonly AppDbContext _db;
        public PermissionService(AppDbContext db) { _db = db; }

        public async Task<HashSet<string>> GetUserPermissionsAsync(User user)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (user.Role == "admin")
            {
                foreach (var c in MenuCatalog.AllCodes()) set.Add(c);
                return set;
            }

            var codes = await _db.RolePermissions
                .Where(rp => rp.Role != null && rp.Role.Code == user.Role)
                .Select(rp => rp.PermissionCode)
                .ToListAsync();
            foreach (var c in codes) set.Add(c);
            return set;
        }
    }
}
