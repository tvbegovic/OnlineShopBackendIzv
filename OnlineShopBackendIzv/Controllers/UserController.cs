using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShopBackendIzv.JWT;
using System.Data.SqlClient;
using System.Security.Claims;

namespace OnlineShopBackendIzv.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IJwtAuthManager jwtAuthManager;

        public UserController(IConfiguration configuration, IJwtAuthManager jwtAuthManager)
        {
            this.configuration = configuration;
            this.jwtAuthManager = jwtAuthManager;
        }

        [HttpGet("login")]
        public IActionResult Login(string username, string password) 
        {
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                var user = db.QueryFirstOrDefault<Employee>(
                    "SELECT * FROM Employee WHERE username = @username AND password = @password",
                    new { username, password });
                if (user == null)
                {
                    return BadRequest("Neispravni korisnički podaci");
                }
                var result = new LoginResult();
                result.User = user;
                var claims = new Claim[] { new Claim(ClaimTypes.Name, username) };
                var jwtResult = jwtAuthManager.GenerateTokens(username, claims, DateTime.Now);
                result.AccessToken = jwtResult.AccessToken;
                result.RefreshToken = jwtResult.RefreshToken.TokenString;
                return Ok(result);
            }
        }
    }

    public class LoginResult
    {
        public Employee? User { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
