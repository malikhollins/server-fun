using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public AuthController( 
        UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _db = db;
    }

    [HttpPost("allow")]
    public async Task<IActionResult> AddAllowedEmailAsync( string email )
    {
        var newEmail = new AllowedEmail{ Email = email };
        await _db.AllowedEmails.AddAsync( newEmail );
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login( [FromForm] LoginForm form)
    {
        var user = await _userManager.FindByEmailAsync(form.Email);
        if (user == null)
        {
            Console.WriteLine( "Could not find email {0}", form.Email );
            bool allowed = await _db.AllowedEmails.AnyAsync(x => x.Email == form.Email);
            if (!allowed)
            {
                return NotFound();
            }

            user = new IdentityUser{ UserName = form.Email?.Split('@')[0], Email = form.Email };
            var createResult = await _userManager.CreateAsync( user, form.Password );
            var roleResult = await _userManager.AddToRoleAsync( user, "allowed" );
            if ( !roleResult.Succeeded || !createResult.Succeeded )
            {
                return NotFound();
            }
        }
        
        var signInResult = await _signInManager.PasswordSignInAsync(
            user.UserName ?? form.Email,
            form.Password,
            isPersistent: true,
            lockoutOnFailure: false);

        return signInResult.Succeeded ? Redirect("/dashboard") : Redirect("/");
    }
}