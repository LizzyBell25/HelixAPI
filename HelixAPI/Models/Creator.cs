using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Model
{
    public class Creator
    {
        [Key]
        public Guid Creator_Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string First_Name { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Last_Name { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Sort_Name { get; set; }
    }
}
