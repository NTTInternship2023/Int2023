using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchTheWebServer.Data;
using SearchTheWebServer.Validators;
using SearchTheWebServer.Models;
using System.Security.Cryptography;

namespace SearchTheWebServer.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
       public UserController(AppDbContext context)
        { 
            _context = context;
        }
                
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterUserDto userDto)
        {
            try
            {
                //Validation block
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);

                if (existingUser != null)
                {
                    return Conflict("Username already exists");
                }

                if (!PasswordValidator.PasswordPatternValidation(userDto.Password)) return Conflict("Invalid format for Password");
                if (!EmailValidator.EmailPatternValidation(userDto.Email)) return Conflict("Invalid format for Email");
                
                //Crypting the password
                CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                User user = new User();
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                //Creating the entity for the db
                User newUser = new User { Username = userDto.Username, Email = userDto.Email, PasswordHash = user.PasswordHash, PasswordSalt = user.PasswordSalt };

                //Adding and saving the entity for the db
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok($"New user created! Welcome, {newUser.Username}!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error registering user: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<(bool, ActionResult<string>)> Login(LoginUserDto userDto)
        {
            try {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
               
                if (existingUser == null )
                {
                    return (false, "Username unregister, please register");
                    //return ("Username unregister, please register");
                }
               
                else if (!VerifyPasswordHash(userDto.Password, existingUser.PasswordHash, existingUser.PasswordSalt))
                {
                    return (false, "Wrong password");
                }
               
                return (true, "Succesful Login");
            }
            catch (Exception ex) {
                return (false, StatusCode(500, $"Error logging in: {ex.Message}"));
            }
        }
        [HttpPost("changepassword")]
        public async Task<ActionResult<User>> ChangePassword(NewPasswordUserDto userDto)
        {
            try
            {
                //validation block
                var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
                if (updatedUser == null)
                {
                    return BadRequest("User not found");
                }
                else if (!VerifyPasswordHash(userDto.Password, updatedUser.PasswordHash, updatedUser.PasswordSalt))
                {
                    return Conflict("Wrong password");
                }
                if (!PasswordValidator.PasswordPatternValidation(userDto.NewPassword)) return Conflict("Invalid format for Password");
                
                CreatePasswordHash(userDto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                updatedUser.PasswordHash = passwordHash;
                updatedUser.PasswordSalt = passwordSalt;
                await _context.SaveChangesAsync();
                return Ok(updatedUser);
            }
            catch (Exception ex) {
                return StatusCode(500, $"Error changing password in: {ex.Message}");
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt) 
        {
            using( var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using( var hmac= new HMACSHA512(passwordSalt))
            {
                var computedHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes((string)password));
               return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}

