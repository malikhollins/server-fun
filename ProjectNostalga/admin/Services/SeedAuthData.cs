
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class SeedAuthData
{
    public static async Task InitializeAsync(IServiceProvider  services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.MigrateAsync();

        var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL")  ?? throw new Exception("ADMIN_EMAIL not set.");
        var adminUser = await userManager.FindByEmailAsync(email);

        await SeedRoleAsync( "admin" );
        await SeedRoleAsync( "allowed" );

        if ( adminUser == null )
        {
            adminUser = new IdentityUser{ UserName = email?.Split('@')[0] , Email = email };
            var createResult = await userManager.CreateAsync( adminUser, Environment.GetEnvironmentVariable("ADMIN_PASS") ?? throw new Exception("ADMIN_PASS not set.") );
            if (!createResult.Succeeded)
            {
                throw new Exception(string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
            await userManager.AddToRoleAsync( adminUser, "admin" );
        }

        await db.SaveChangesAsync();

        return;

        async Task SeedRoleAsync( string role )
        {
            var allowedExists = await roleManager.RoleExistsAsync(role );
            if ( !allowedExists )
            {
                var identityRole = new IdentityRole{ Name = role };
                await roleManager.CreateAsync( identityRole );
            }
        }
    }
}
