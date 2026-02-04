using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public enum UserRole
    {
        Admin,
        Cashier,
        Salesperson
    }

    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public UserRole Role { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
