using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FinalProject.Dtos;

namespace FinalProject.Controllers
{
    public class TokenFactoryService
    {
        [Route("api/[controller]")]
        [ApiController]
        public class Login_RegisterController : ControllerBase
        {
            private IConfiguration _config;

            public Login_RegisterController(IConfiguration configuration)
            {
                _config = configuration;
            }

            private JwtDto AuthenticateUser(JwtDto phonenumber)
            {
                JwtDto _phonenumber = null;

                _phonenumber = new JwtDto { PhoneNumber = phonenumber.PhoneNumber };

                return _phonenumber;
            }

            private string GenerateToken(JwtDto phonenumbers)
            {
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(_config["Jwt:Key"],
                    _config["Jwt:Audience"], null,
                    expires: DateTime.Now.AddSeconds(85),
                    signingCredentials: credentials);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            [AllowAnonymous]
            [HttpPost]
            public IActionResult Login(JwtDto phonenumber)
            {
                IActionResult response = Unauthorized();
                var phonenumber_ = AuthenticateUser(phonenumber);
                if (phonenumber_ != null)
                {
                    var token = GenerateToken(phonenumber_);
                    response = Ok(new { token });
                }
                return response;
            }
        }
    }
}