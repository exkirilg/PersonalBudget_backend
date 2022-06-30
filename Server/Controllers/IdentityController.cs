using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public IdentityController(IConfiguration config, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _config = config;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp(SignupDTO signupDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        if ((await _userManager.FindByEmailAsync(signupDTO.Email)) is not null)
        {
            ModelState.AddModelError("Email", "Email is already taken");
            return BadRequest(ModelState);
        }

        var user = new IdentityUser { UserName = signupDTO.Email.Split('@')[0], Email = signupDTO.Email };
        await _userManager.CreateAsync(user, signupDTO.Password);
        await _userManager.AddToRoleAsync(user, "User");

        var claims = await GetUserClaims(user);
        var expirationDate = DateTime.UtcNow.AddMonths(1);
        return Ok(
            new
            {
                Token = GetAccessToken(claims, expirationDate),
                ExpirationDate = expirationDate,
                UserName = user.UserName,
                IsAdmin = claims.Where(claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin").Any()
            });
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(SigninDTO signinDTO)
    {
        var user = await _userManager.FindByEmailAsync(signinDTO.Email);

        if (user is null || (await CheckPassword(user, signinDTO)) == false)
        {
            ModelState.AddModelError("Authentication", "Incorrect email or password");
            return BadRequest(ModelState);
        }

        var claims = await GetUserClaims(user);
        var expirationDate = DateTime.UtcNow.AddMonths(1);
        return Ok(
            new
            { 
                Token = GetAccessToken(claims, expirationDate),
                ExpirationDate = expirationDate,
                UserName = user.UserName,
                IsAdmin = claims.Where(claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin").Any()
            });
    }

    private async Task<bool> CheckPassword(IdentityUser user, SigninDTO signinDTO)
    {
        return (await _signInManager.CheckPasswordSignInAsync(user, signinDTO.Password, false)).Succeeded;
    }
    private async Task<IEnumerable<Claim>> GetUserClaims(IdentityUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return claims;
    }
    private string GetAccessToken(IEnumerable<Claim> claims, DateTime expirationDate)
    {
        var handler = new JwtSecurityTokenHandler();
        
        var secret = Encoding.ASCII.GetBytes(_config["Auth:JWTSecret"]);
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _config["Auth:Issuer"],
            Audience = _config["Auth:Audience"],
            Expires = expirationDate,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = signingCredentials
        };
        
        var token = handler.CreateJwtSecurityToken(descriptor);
        return handler.WriteToken(token);
    }
}
