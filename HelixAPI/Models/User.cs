using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Model
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Password { get; set; }

        [Required]
        public required bool Active { get; set; } = true;
    }
}
