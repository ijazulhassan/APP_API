using API.Data;
using API.DTOs;
using API.Entitites;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("register")]

        public async Task<ActionResult<UserResponse>> Register([FromBody] RequestRegister register)
        {
            if (_context.Users.Any(n =>
            n.UserName == register.Username))
                return BadRequest("user already taken");


            var user = new User
            {
                UserName = register.Username.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
           
            var response = new UserResponse
            { 
               Username = user.UserName,
               Token = _tokenService.CreateToken(user)
            };

            return response;
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<UserResponse>> Login(LoginRequest login)
        {
            var user = await _context.Users.
                SingleOrDefaultAsync(n => n.UserName == login.Username);
            if (user == null) return Unauthorized("Invalid Username");

            var response = new UserResponse
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };

            return BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash) == true ? response : BadRequest("wrong password");

            
        }
    }
}
