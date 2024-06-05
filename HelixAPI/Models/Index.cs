using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Model
{
    public class Index
    {
        [Key]
        public Guid Index_Id { get; set; }

        [Required]
        public required Guid Entity_Id { get; set; }

        [Required]
        public required Guid Indexed_By { get; set; }

        [Required]
        public required Guid Source_Id { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Location { get; set; }

        [Required]
        public required Subject Subject { get; set; }
    }
}
