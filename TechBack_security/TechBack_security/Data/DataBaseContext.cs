using Microsoft.EntityFrameworkCore;
using TechBack_security.Models.Entity;

namespace TechBack_security.Data
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Comment> comments { get; set; }
    }
}
