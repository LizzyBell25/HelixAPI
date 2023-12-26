using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HelixAPI.Model
{
    [PrimaryKey(nameof(ID), nameof(EntityID))]
    public class Entity
    {
        public Entity(Guid ID, int EntityID, string Name, string Description, Category Category)
        {
            this.ID = ID;
            this.EntityID = EntityID;
            this.Name = Name;
            this.Description = Description;
            this.Category = Category;
        }

        public Entity(int EntityID, string Name, string Description, Category Category)
        {
            this.ID = new Guid();
            this.EntityID = EntityID;
            this.Name = Name;
            this.Description = Description;
            this.Category = Category;
        }

        public Guid ID { get; private set; }

        public int EntityID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Category Category { get; set; }
    }

    public enum Category
    {
        God = 0,
        Aesir = 1,
        Vanir = 2,
        Jotun = 4,
        Animal = 8,
        Hero = 16,
        Valkyrie = 32,
        place = 64
    }
}
