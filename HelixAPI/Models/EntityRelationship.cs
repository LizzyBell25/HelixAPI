using Microsoft.EntityFrameworkCore;

namespace HelixAPI.Model
{
    [PrimaryKey(nameof(ID))]
    public class EntityRelationship
    {
        public EntityRelationship(Guid ID, Guid Entity1ID, Guid Entity2ID, Relationship Relationship)
        {
            this.ID = ID;
            this.Entity1ID = Entity1ID;
            this.Entity2ID = Entity2ID;
            this.Relationship = Relationship;
        }

        public EntityRelationship(Guid Entity1ID, Guid Entity2ID, Relationship Relationship)
        {
            this.ID = new Guid();
            this.Entity1ID = Entity1ID;
            this.Entity2ID = Entity2ID;
            this.Relationship = Relationship;
        }

        public Guid ID { get; private set; }

        public Guid Entity1ID { get; set; }

        public Guid Entity2ID { get; set; }

        public Relationship Relationship { get; set; }
    }

    public enum Relationship
    {
        Spouse = 0,
        Sibling = 1,
        Child = 2,
        ByName = 4,
        Cognate = 8,
        Enemy = 16
    }
}
