using HelixAPI.Model;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Model
{
    public class Source
    {
        [Key]
        public Guid SourceId { get; set; }

        [Required]
        public DateTime PublicationDate { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Publisher { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Url { get; set; }

        [Required]
        public Guid CreatorId { get; set; }

        [Required]
        public Branch Branch { get; set; }

        [Required]
        public Content ContentType { get; set; }

        [Required]
        public Flag Flags { get; set; }

        [Required]
        public Format Format { get; set; }
    }
}
