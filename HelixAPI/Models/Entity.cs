using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Models
{
    public class Entity
    {
        [Key]
        public Guid Entity_Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public Catagory Type { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }  // Concurrency token
    }
}
