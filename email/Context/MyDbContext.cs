using Microsoft.EntityFrameworkCore;
using email.Models;

namespace email.Context
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        // DbSet properties for your entities
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Recipient> Recipients { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<Admin> Admins {get;set;}
    }
}