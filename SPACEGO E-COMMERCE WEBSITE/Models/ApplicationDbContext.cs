using Microsoft.EntityFrameworkCore;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Customer> Customers { get; set; }

    }
}
