using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using tugasmodul7.Models;

namespace tugasmodul7.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
