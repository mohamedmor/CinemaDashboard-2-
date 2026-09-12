using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CinemaDashboard.Models;

namespace CinemaDashboard.Data
{
    // Inherits from IdentityDbContext so the Identity tables (AspNetUsers, AspNetRoles, ...)
    // live in the same database and the same DbContext as the rest of the app.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieImage> MovieImages { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Movie -> Category (many-to-one)
            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Movies)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Movie -> Cinema (many-to-one)
            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Cinema)
                .WithMany(c => c.Movies)
                .HasForeignKey(m => m.CinemaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Movie -> SubImages (one-to-many)
            modelBuilder.Entity<MovieImage>()
                .HasOne(mi => mi.Movie)
                .WithMany(m => m.SubImages)
                .HasForeignKey(mi => mi.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Movie <-> Actor (many-to-many via MovieActor)
            modelBuilder.Entity<MovieActor>(entity =>
            {
                entity.HasKey(ma => new { ma.MovieId, ma.ActorId });

                entity.HasOne(ma => ma.Movie)
                    .WithMany(m => m.MovieActors)
                    .HasForeignKey(ma => ma.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ma => ma.Actor)
                    .WithMany(a => a.MovieActors)
                    .HasForeignKey(ma => ma.ActorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Order -> OrderItems (one-to-many)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem -> Movie (many-to-one, restrict so movie deletion doesn't cascade into order history)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Movie)
                .WithMany()
                .HasForeignKey(oi => oi.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
