using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Projecttask.Models;
using System;
using System.Reflection.Emit;

namespace Projecttask.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Orders> Orders { get; set; }
    public DbSet<UserTag> UserTag { get; set; }
    public DbSet<Tag> Tag { get; set; }
}

public class YourDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Data Source=STHQ0126-07;Initial Catalog=Iusta;User ID=admin;Password=admin;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
