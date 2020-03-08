using Microsoft.EntityFrameworkCore;
using System;
using Timetracker.Entities.Models;

namespace Timetracker.Entities.Classes
{
    public class TimetrackerContext : DbContext, IDisposable
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Right> Rights { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<AuthorizedUser> AuthorizedUsers { get; set; }

        public TimetrackerContext(DbContextOptions options) : base(options)
        {
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
