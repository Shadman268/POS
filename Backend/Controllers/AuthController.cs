using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private const string RefreshTokenCookieName = "refreshToken";

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (response, refreshToken) = await _authService.LoginAsync(request);

                // Set refresh token in HttpOnly cookie
                SetRefreshTokenCookie(refreshToken.Token, refreshToken.Expires);

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies[RefreshTokenCookieName];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { message = "Refresh token not found" });
                }

                var result = await _authService.RefreshTokenAsync(refreshToken);

                if (result == null)
                {
                    // Clear the invalid cookie
                    DeleteRefreshTokenCookie();
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                var (response, newRefreshToken) = result.Value;

                // Set new refresh token in HttpOnly cookie
                SetRefreshTokenCookie(newRefreshToken.Token, newRefreshToken.Expires);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during token refresh", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshToken = Request.Cookies[RefreshTokenCookieName];

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await _authService.RevokeTokenAsync(refreshToken);
                }

                // Clear the refresh token cookie
                DeleteRefreshTokenCookie();

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during logout", error = ex.Message });
            }
        }

        [HttpGet("verify")]
        [Authorize]
        public IActionResult Verify()
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new { username, role });
        }

        private void SetRefreshTokenCookie(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires,
                SameSite = SameSiteMode.None,
                Secure = true, // Required when SameSite=None
                Path = "/"
            };

            Response.Cookies.Append(RefreshTokenCookieName, token, cookieOptions);
        }

        private void DeleteRefreshTokenCookie()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(-1),
                SameSite = SameSiteMode.None,
                Secure = true,
                Path = "/"
            };

            Response.Cookies.Append(RefreshTokenCookieName, "", cookieOptions);
        }
    }
}
