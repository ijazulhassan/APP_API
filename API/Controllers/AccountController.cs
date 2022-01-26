using API.Data;
using API.DTOs;
using API.Entitites;
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

        public AccountController(DataContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("register")]

        public async Task<ActionResult<User>> Register([FromBody] RequestRegister register)
        {
            if (_context.Users.Any(n => n.UserName == register.Username)) return BadRequest("user already taken");


            var user = new User
            {
                UserName = register.Username.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password)
        };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<User>> Login(LoginRequest login)
        {
            var user = await _context.Users.
                SingleOrDefaultAsync(n => n.UserName == login.Username);
            if (user == null) return Unauthorized("Invalid Username");

            return BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash) == true ? user : BadRequest("wrong password");
        }


    }
}
