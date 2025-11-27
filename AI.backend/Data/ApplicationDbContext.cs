using Microsoft.EntityFrameworkCore;
using AI.backend.Models;

namespace AI.backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Comment> Comments { get; set; } // Add this line
}