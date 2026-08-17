using System.ComponentModel.DataAnnotations;

namespace QuanliERP.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Username { get; set; } = "";
        [Required]
        public string PasswordHash { get; set; } = "";
        [MaxLength(50)]
        public string DisplayName { get; set; } = "";
        [MaxLength(30)]
        public string Role { get; set; } = "user";
        [MaxLength(20)]
        public string Phone { get; set; } = "";
        [MaxLength(100)]
        public string Email { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
