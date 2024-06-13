using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Models
{
    public class EntityRelationship
    {
        [Key]
        public Guid Relationship_Id { get; set; }

        [Required]
        public required Guid Entity1_Id { get; set; }

        [Required]
        public required Guid Entity2_Id { get; set; }

        [Required]
        public required RelationshipType Relationship_Type { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }  // Concurrency token
    }
}
