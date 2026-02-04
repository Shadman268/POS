using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _refreshTokenExpirationDays = 7;
        private readonly int _accessTokenExpirationMinutes = 15;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(LoginResponse Response, RefreshToken RefreshToken)> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            var accessToken = GenerateJwtToken(user.Id, user.Username, user.Role.ToString());
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    Role = user.Role.ToString().ToLower()
                }
            };

            return (response, refreshToken);
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists");
            }

            // Parse role
            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            {
                throw new InvalidOperationException("Invalid role specified");
            }

            // Create new user
            var user = new User
            {
                Username = request.Username,
                PasswordHash = HashPassword(request.Password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new RegisterResponse
            {
                Message = "Registration successful",
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    Role = user.Role.ToString().ToLower()
                }
            };
        }

        public async Task<(RefreshTokenResponse Response, RefreshToken RefreshToken)?> RefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (existingToken == null || !existingToken.IsActive)
            {
                return null;
            }

            // Revoke the old refresh token
            existingToken.Revoked = DateTime.UtcNow;

            // Generate new tokens
            var newRefreshToken = await GenerateRefreshTokenAsync(existingToken.UserId);
            existingToken.ReplacedByToken = newRefreshToken.Token;

            await _context.SaveChangesAsync();

            var user = existingToken.User;
            var accessToken = GenerateJwtToken(user.Id, user.Username, user.Role.ToString());

            var response = new RefreshTokenResponse
            {
                AccessToken = accessToken,
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    Role = user.Role.ToString().ToLower()
                }
            };

            return (response, newRefreshToken);
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            var existingToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (existingToken != null && existingToken.IsActive)
            {
                existingToken.Revoked = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public string GenerateJwtToken(int userId, string username, string role)
        {
            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync(int userId)
        {
            // Revoke any existing active refresh tokens for this user
            var existingTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in existingTokens)
            {
                token.Revoked = DateTime.UtcNow;
            }

            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureToken(),
                UserId = userId,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays)
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
