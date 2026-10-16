using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicApi.Data;
using ClinicApi.DTOs;
using ClinicApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ClinicApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration; //key 

        public AuthService(AppDbContext context,IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(RegisterRequestDto request)
        {
    
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return "Error: User with this email already exists!";
            }

            
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), 
                RoleId = request.RoleId,
                CreatedAt = DateTime.UtcNow 
            };

            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return "Success: User registered successfully!";
        }

        public async Task<string> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return "Error: Invalid email or password.";
            }

          
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return "Error: Invalid email or password.";
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.Name) // User ka Role (e.g. "Admin") token mein daal diya
                }),
                Expires = DateTime.UtcNow.AddMinutes(15), // Senior ki requirement ke mutabiq 15 minute ka expiry time
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
            
        }
    }
}