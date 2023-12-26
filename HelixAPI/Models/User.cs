using Microsoft.EntityFrameworkCore;

namespace HelixAPI.Model
{
    [PrimaryKey(nameof(ID))]
    public class User
    {
        public User(Guid ID, string Name, string Email, bool Active)
        {
            this.ID = ID;
            this.Name = Name;
            this.Email = Email;
            this.Active = Active;
        }

        public User(string Name, string Email, bool Active)
        {
            this.ID = new Guid();
            this.Name = Name;
            this.Email = Email;
            this.Active = Active;
        }

        public Guid ID { get; private set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public bool Active { get; set; }
    }
}
