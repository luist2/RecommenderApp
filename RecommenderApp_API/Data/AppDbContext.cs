using Microsoft.EntityFrameworkCore;
using RecommenderApp_API.Entities;

namespace RecommenderApp_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<UserMovie> UserMovies => Set<UserMovie>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the User entity
            modelBuilder.Entity<User>(entity =>
            {
                // Create a unique index on the Username property
                entity.HasIndex(u => u.Username).IsUnique();
            });

            // Configure the UserMovie entity to establish a relationship with the User entity
            modelBuilder.Entity<UserMovie>()
                .HasOne(um => um.User) // Specify that UserMovie has one User
                .WithMany(u => u.UserMovies) // Specify that User has many UserMovies
                .HasForeignKey(um => um.UserId); // Set the foreign key for UserId

            // Configure the UserMovie entity to establish a relationship with the Movie entity
            modelBuilder.Entity<UserMovie>()
                .HasOne(um => um.Movie) // Specify that UserMovie has one Movie
                .WithMany(m => m.UserMovies) // Specify that Movie has many UserMovies
                .HasForeignKey(um => um.MovieId); // Set the foreign key for MovieId
        }
    }
}

