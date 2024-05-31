using HelixAPI.Model;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Model
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
    }
}
