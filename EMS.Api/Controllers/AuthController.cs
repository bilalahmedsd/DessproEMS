using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EMS.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUsersRepository UserRepository;
        public AuthController(IConfiguration config, IUsersRepository userRepository, IUserServices services) 
        {
            _config = config;
            UserRepository = userRepository;

        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            ResponseModel resp = new ResponseModel();

            try
            {
                UserDTO userDTO = await UserRepository.Validate(username, password);
                if (userDTO != null)
                {
                    userDTO.JwtToken = GenerateJwtToken(userDTO.FkUserId.ToString());
                    resp.Message = ConstantMessages.LoginSuccessMessage;
                    resp.IsSuccess = true;
                    resp.Data = userDTO;
                }
                else
                {
                    resp.IsSuccess = false;
                    resp.Message = ConstantMessages.UnauthroizedMessage;
                }
            }
            catch (Exception ex)
            {
                resp.Message = ConstantMessages.ErrorMessage;
                resp.IsSuccess = false;
            }

            return Ok(resp);

        }
        private string GenerateJwtToken(string username)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin") // Assign roles dynamically
                }),
                Expires = DateTime.UtcNow.AddHours(2), // Token expiry
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
