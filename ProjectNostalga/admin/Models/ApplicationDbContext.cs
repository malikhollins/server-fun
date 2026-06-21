using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AllowedEmail> AllowedEmails => Set<AllowedEmail>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AllowedEmail>()
               .HasIndex(x => x.Email)
               .IsUnique();
               
        builder.Entity<AllowedEmail>()
               .Property(x => x.Email)
               .HasMaxLength(256);
    }
}