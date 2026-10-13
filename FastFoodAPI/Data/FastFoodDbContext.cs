using FastFoodAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FastFoodAPI.Data
{
    public class FastFoodDbContext : DbContext
    {
        public FastFoodDbContext(
            DbContextOptions<FastFoodDbContext> options)
            : base(options) 
        {
        }

        public DbSet<Area> Areas => Set<Area>();    //DbSet<Area> and DbSet<Order> represent the entities we want to store in the database.
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Area>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(80);

                entity.Property(a => a.Acronym)
                .IsRequired()
                .HasMaxLength(1);

                entity.HasIndex(a => a.Acronym)
                .IsUnique();

                entity.HasData                      //HasData() defines the two initial areas that will be inserted by the migration.
                (
                    new Area
                    {
                        Id = 1,
                        Name = "Sala",
                        Acronym = "S"
                    },
                    new Area
                    {
                        Id = 2,
                        Name = "Esplanada",
                        Acronym = "E"
                    }
                );
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                
                entity.Property(o => o.Code)
                .IsRequired()
                .HasMaxLength(20);

                entity.HasIndex(o => new { o.AreaId, o.Code }) 
                .IsUnique();

                entity.HasOne(o => o.Area)
                .WithMany(a => a.Orders)
                .HasForeignKey(o => o.AreaId)           // HasOne() and WithMany() establish the relationship between orders and areas.
                .OnDelete(DeleteBehavior.Restrict);

            });
        }
    }
}
