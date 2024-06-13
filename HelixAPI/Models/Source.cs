using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Models
{
    public class Source
    {
        [Key]
        public Guid Source_Id { get; set; }

        [Required]
        public required Guid Creator_Id { get; set; }

        [Required]
        public required DateTime Publication_Date { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Publisher { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Url { get; set; }

        [Required]
        public required Branch Branch { get; set; }

        [Required]
        public required ContentType Content_Type { get; set; }

        public Flag Flags { get; set; }

        [Required]
        public required Format Format { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }  // Concurrency token
    }
}
