using Microsoft.EntityFrameworkCore;

namespace HelixAPI.Model
{
    [PrimaryKey(nameof(ID), nameof(IndexID))]
    public class Index
    {
        public Index(Guid ID, int IndexID, Guid EntityID, Guid IndexedByID, string Location, Guid SourceID, Subject Subject)
        {
            this.ID = ID;
            this.IndexID = IndexID;
            this.EntityID = EntityID;
            this.IndexedByID = IndexedByID;
            this.Location = Location;
            this.SourceID = SourceID;
            this.Subject = Subject;
        }

        public Index(int IndexID, Guid EntityID, Guid IndexedByID, string Location, Guid SourceID, Subject Subject) 
        {
            this.ID = new Guid();
            this.IndexID = IndexID;
            this.EntityID = EntityID;
            this.IndexedByID = IndexedByID;
            this.Location = Location;
            this.SourceID = SourceID;
            this.Subject = Subject;
        }

        public Guid ID { get; private set; }

        public int IndexID { get; set; }

        public Guid EntityID { get; set; }

        public Guid IndexedByID { get; set; }

        public string Location { get; set; }

        public Guid SourceID { get; set; }

        public Subject Subject { get; set; }
    }

    public enum Subject
    {
        Afterlife = 0,
        Luck = 1,
        Ancestors = 2,
        Oaths = 4,
        Valkyrie = 8,
        BurialPractices = 16,
        Magic = 32,
        Jotun = 64,
        FemaleVikings = 128,
        MythicalCreature = 256
    }
}
