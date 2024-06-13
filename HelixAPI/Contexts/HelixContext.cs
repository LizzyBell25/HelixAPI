using Microsoft.EntityFrameworkCore;
using HelixAPI.Models;

namespace HelixAPI.Contexts
{
    public class HelixContext(DbContextOptions<HelixContext> options) : DbContext(options)
    {
        public virtual DbSet<Entity> Entities { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Creator> Creators { get; set; }
        public virtual DbSet<Source> Sources { get; set; }
        public virtual DbSet<Models.Index> Indexes { get; set; }
        public virtual DbSet<EntityRelationship> EntityRelationships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>().ToTable("Entities");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Creator>().ToTable("Creators");
            modelBuilder.Entity<Source>().ToTable("Sources");
            modelBuilder.Entity<Models.Index>().ToTable("Indexes");
            modelBuilder.Entity<EntityRelationship>().ToTable("Entity_Relationship");

            // Enums to string conversions
            modelBuilder.Entity<Entity>().Property(e => e.Type).HasConversion<string>();
            modelBuilder.Entity<Source>().Property(s => s.Branch).HasConversion<string>();
            modelBuilder.Entity<Source>().Property(s => s.Content_Type).HasConversion<string>();
            modelBuilder.Entity<Source>().Property(s => s.Flags).HasConversion<string>();
            modelBuilder.Entity<Source>().Property(s => s.Format).HasConversion<string>();
            modelBuilder.Entity<Models.Index>().Property(i => i.Subject).HasConversion<string>();
            modelBuilder.Entity<EntityRelationship>().Property(r => r.Relationship_Type).HasConversion<string>();

            modelBuilder.Entity<Entity>().Property(e => e.RowVersion).IsRowVersion();
            modelBuilder.Entity<User>().Property(u => u.RowVersion).IsRowVersion();
            modelBuilder.Entity<Creator>().Property(c => c.RowVersion).IsRowVersion();
            modelBuilder.Entity<Source>().Property(s => s.RowVersion).IsRowVersion();
            modelBuilder.Entity<Models.Index>().Property(i => i.RowVersion).IsRowVersion();
            modelBuilder.Entity<EntityRelationship>().Property(r => r.RowVersion).IsRowVersion();
        }
    }
}