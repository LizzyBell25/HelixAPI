using Microsoft.EntityFrameworkCore;
using HelixAPI.Model;

namespace helixapi.Data
{
    public class HelixContext : DbContext
    {
        public HelixContext(DbContextOptions<HelixContext> options) : base(options) { }

        public DbSet<Entity> Entities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Creator> Creators { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<HelixAPI.Model.Index> Indexes { get; set; }
        public DbSet<EntityRelationship> EntityRelationships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>().ToTable("Entities");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Creator>().ToTable("Creators");
            modelBuilder.Entity<Source>().ToTable("Sources");
            modelBuilder.Entity<HelixAPI.Model.Index>().ToTable("Indexes");
            modelBuilder.Entity<EntityRelationship>().ToTable("Entity_Relationship");

            // Enums to string conversions
            modelBuilder.Entity<Entity>().Property(e => e.Type).HasConversion<string>();
            modelBuilder.Entity<Source>().Property(s => s.Branch).HasConversion<string>();
            modelBuilder.Entity<Source>().Property(s => s.ContentType).HasConversion<string>();
            modelBuilder.Entity<Source>().Property(s => s.Flags).HasConversion<string>();
            modelBuilder.Entity<Source>().Property(s => s.Format).HasConversion<string>();
            modelBuilder.Entity<HelixAPI.Model.Index>().Property(i => i.Subject).HasConversion<string>();
            modelBuilder.Entity<EntityRelationship>().Property(r => r.RelationshipType).HasConversion<string>();
        }
    }
}