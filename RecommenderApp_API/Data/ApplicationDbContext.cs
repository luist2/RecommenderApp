using Microsoft.EntityFrameworkCore;
using RecommenderApp_API.Entities;

namespace RecommenderApp_API.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

    }
}
