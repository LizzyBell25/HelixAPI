using HelixAPI.Model;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Model
{
    public class Index
    {
        [Key]
        public Guid IndexId { get; set; }

        [Required]
        public Guid EntityId { get; set; }

        [Required]
        public Guid IndexedBy { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Location { get; set; }

        [Required]
        public Guid SourceId { get; set; }

        [Required]
        public Subject Subject { get; set; }
    }
}
