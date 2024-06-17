using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DemoJWT.DatabaseContext;
using DemoJWT.Helpers;
using DemoJWT.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DemoJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest model)
        {
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                return BadRequest("Username already exists.");
            }

            var hashedPasswordAndSalt = SecurityHelpers.GetHashedPasswordAndSalt(model.Password);

            var user = new AppUser()
            {
                Username = model.Username,
                Password = hashedPasswordAndSalt.Item1,
                Salt = hashedPasswordAndSalt.Item2,
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully.");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == loginRequest.Username);
            if (user == null)
            {
                return Unauthorized("Invalid login or password.");
            }

            var passwordHashFromDb = user.Password;
            var curHashedPassword = SecurityHelpers.GetHashedPasswordWithSalt(loginRequest.Password, user.Salt);

            if (passwordHashFromDb != curHashedPassword)
            {
                return Unauthorized("Invalid login or password.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "https://localhost:5001",
                audience: "https://localhost:5001",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var refreshToken = SecurityHelpers.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExp = DateTime.Now.AddDays(7);
            _context.SaveChanges();

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = refreshToken
            });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult Refresh(RefreshTokenRequest refreshTokenRequest)
        {
            var user = _context.Users.FirstOrDefault(u => u.RefreshToken == refreshTokenRequest.RefreshToken);
            if (user == null || user.RefreshTokenExp < DateTime.Now)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: "https://localhost:5001",
                audience: "https://localhost:5001",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var newRefreshToken = SecurityHelpers.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExp = DateTime.Now.AddDays(7);
            _context.SaveChanges();

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                refreshToken = newRefreshToken
            });
        }

        [Authorize]
        [HttpGet("secret")]
        public IActionResult GetSecretData()
        {
            return Ok("Giga prywatne dane które nie powinny wycieknąć.");
        }
    }
}
