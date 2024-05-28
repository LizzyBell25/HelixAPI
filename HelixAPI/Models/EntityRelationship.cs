using helixapi.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Model
{
    public class EntityRelationship
    {
        [Key]
        public Guid RelationshipId { get; set; }

        [Required]
        public Guid Entity1Id { get; set; }

        [Required]
        public Guid Entity2Id { get; set; }

        [Required]
        public RelationshipType RelationshipType { get; set; }
    }
}
