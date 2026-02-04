using Backend.DTOs;
using Backend.Models;

namespace Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(LoginResponse Response, RefreshToken RefreshToken)> LoginAsync(LoginRequest request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<(RefreshTokenResponse Response, RefreshToken RefreshToken)?> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string refreshToken);
        string GenerateJwtToken(int userId, string username, string role);
    }
}
