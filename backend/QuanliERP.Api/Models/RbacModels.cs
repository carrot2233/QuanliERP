using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    // 角色
    public class Role
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; } = "";
        [Required, MaxLength(50)]
        public string Code { get; set; } = "";
        [MaxLength(200)]
        public string Description { get; set; } = "";
        public bool IsBuiltIn { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // 角色-菜单权限（权限码 -> 角色）
    public class RolePermission
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        [MaxLength(100)]
        public string PermissionCode { get; set; } = "";
    }
}
