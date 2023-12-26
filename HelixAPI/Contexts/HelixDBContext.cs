using HelixAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace HelixAPI.DataModels
{
    public class HelixDBContext : DbContext
    {
        public HelixDBContext(DbContextOptions<HelixDBContext> options) : base(options)
        {
        }

        protected HelixDBContext()
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Entity> Entities { get; set; }
        public DbSet<Creator> Creators { get; set; }
        public DbSet<EntityRelationship> EntityRelationships { get; set; }
        public DbSet<Model.Index> Indexs { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
