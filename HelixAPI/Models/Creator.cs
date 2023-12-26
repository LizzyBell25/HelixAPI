using Microsoft.EntityFrameworkCore;

namespace HelixAPI.Model
{
    [PrimaryKey(nameof(ID), nameof(CreatorID))]
    public class Creator
    {
        public Creator(Guid ID, int CreatorID, string Firstname, string Lastname, string SortName)
        {
            this.ID = ID;
            this.CreatorID = CreatorID;
            this.Firstname = Firstname;
            this.Lastname = Lastname;
            this.SortName = SortName;
        }

        public Creator(int CreatorID, string Firstname, string Lastname, string SortName)
        {
            this.ID = new Guid();
            this.CreatorID = CreatorID;
            this.Firstname = Firstname;
            this.Lastname = Lastname;
            this.SortName = SortName;
        }

        
        public Guid ID { get; private set; }

        public int CreatorID { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set;}

        public string SortName { get; set; }
    }
}
