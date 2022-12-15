using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Data;

public class FamilyDbContext : DbContext
{
    public virtual DbSet<Family> Families { get; set; }
    public virtual DbSet<FamilyRole> FamilyRoles { get; set; }
    public virtual DbSet<UserFamilyRole> UserFamilyRoles { get; set; }

    public FamilyDbContext() { }

    public FamilyDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("family");
        modelBuilder.Entity<UserFamilyRole>().HasKey(x => new { x.UserId, x.FamilyId });
        modelBuilder.Seed();
    }
}