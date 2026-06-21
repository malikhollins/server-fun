
Simple auth in .net


add new ef db context for identity users

```c#
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
```


then add this,

```c#
builder.Services.AddAuthorization(); 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/Forbidden/";
    });
```

then add login service


```c#
    public AuthController( 
        UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager,
        ApplicationDbContext db )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    public async Task<IdentityResult> CreateAsync( string email, string password )
    {
        var user = new IdentityUser{ Email = email };
        return await _userManager.CreateAsync( user, password );
    }
    
    public async Task<bool> LoginAsync( string email, string password )
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        return result.Succeeded;
    }
```