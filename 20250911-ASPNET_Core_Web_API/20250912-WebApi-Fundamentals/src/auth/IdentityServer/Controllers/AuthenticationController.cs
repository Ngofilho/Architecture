using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityServer.Controllers
{
    [Route("authenticate")]
    [Route("[controller]")]
    [Route("auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public AuthenticationController(IConfiguration configuration) =>
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        [HttpPost]
        public ActionResult<string> Authenticate([FromBody] UserCredentials userInfo)
        {
            if (userInfo == null || string.IsNullOrEmpty(userInfo.UserName) || string.IsNullOrEmpty(userInfo.Password))
            {
                return BadRequest("Invalid user information.");
            }
            
            var user = ValidateUserPassword(userInfo.UserName, userInfo.Password);
            
            if (user == null) return Unauthorized("Authentication failed. Invalid username or password.");

            return Ok(GenerateToken(user));
        }

        [HttpGet]
        public ActionResult<string> Authenticate([FromQuery] string user, [FromQuery] string password)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                return BadRequest("Invalid user information.");
            }

            var userInfo = ValidateUserPassword(user, password);

            if (userInfo == null) return Unauthorized("Authentication failed. Invalid username or password.");
            
            string tokenToReturn = GenerateToken(userInfo);

            return Ok(tokenToReturn);
        }

        private string GenerateToken(UserInfo userInfo)
        {
            var securityKey = new SymmetricSecurityKey(
                        Convert.FromBase64String(this.configuration["Authentication:SecretForKey"]!));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsForToken = new List<Claim>
            {
                new Claim("sub", "1"),
                new Claim("role", userInfo.Role!),
                new Claim("give_name",userInfo.GivenName!),
                new Claim("family_name",userInfo.FamilyName!),
                new Claim("city", userInfo.City!)
            };

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: userInfo.Issuer,
                audience: userInfo.Audience,
                claims: claimsForToken,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(jwtSecurityToken); 
        }

        private UserInfo? ValidateUserPassword(string userName, string password)
        {
            if (password.ToLower().Trim() != "palmeiras"
                && password.ToLower().Trim() != "verdão"
                && password.ToLower().Trim() != "verdao"
                && password.ToLower().Trim() != "porco")
                return null;

            return new UserInfo { 
                UserName = userName,
                Audience = "cityinfoapi", City = "Antwerp",
                FamilyName ="Dockx", GivenName = "Kevin",
                Issuer = "https://localhost:7169", Role = "Torcedor" };
        }

        public record UserCredentials
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
        }

        private sealed record UserInfo
        {
            public string? UserName { get; set; }            
            public string? Issuer { get; init; }
            public string? City { get; init; }
            public string? Audience { get; init; }
            public string? FamilyName { get; init; }
            public string? GivenName { get; init; }
            public string? Role { get; init; }
        }
    }
}
