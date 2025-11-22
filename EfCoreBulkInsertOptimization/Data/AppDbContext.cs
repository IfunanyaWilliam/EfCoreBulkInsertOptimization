using EfCoreBulkInsertOptimization.Models;
using Microsoft.EntityFrameworkCore;

namespace EfCoreBulkInsertOptimization.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }
}
