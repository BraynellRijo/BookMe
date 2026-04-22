using Application.DTOs.AuthDTOs;
using Application.DTOs.TokensDTOs;
using Application.Interfaces.Services.AuthServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO request)
        {
            var user = await _authService.RegisterAsync(request);
            return Ok(new { message = "User registered successfully.", user });
        }


        [HttpPost("send-verification-email")]
        [Authorize]
        public async Task<IActionResult> SendVerificationEmail([FromBody] string email)
        {
            await _authService.SendVerificationEmailAsync(email);
            return Ok(new { message = "Verification email sent successfully." });
        }


        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDTO request)
        {
            var result = await _authService.VerifyCode(request);

            if (!result)
                return BadRequest(new { message = "Invalid verification code." });

            return Ok(new { message = "Email verified successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO request)
        {
            var result = await _authService.LoginAsync(request);

            if (result is null)
                return BadRequest(new { message = "Email or password is wrong!!" });

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RequestRefreshTokenDTO request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            if (result is null
                || result.Accesstoken is null
                || result.RefreshToken is null)
            {
                return Unauthorized(new { message = "Invalid refresh token!!" });
            }

            return Ok(result);
        }

        [HttpPatch("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = GetUserId();

            await _authService.LogoutAsync(userId);
            return Ok(new { message = "Logged out successfully!" });

        }

        [HttpPatch("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO request)
        {
            var userId = GetUserId();
            await _authService.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Password changed successfully!" });
        }

        [HttpPatch("become-host")]
        [Authorize]
        public async Task<IActionResult> BecomeHost()
        {
            var userId = GetUserId();

            var result = await _authService.BecomeHostAsync(userId);
            return Ok(new { message = "You are now a Host!", token = result });
        }


        private Guid GetUserId()
        {
            var userIdFromToken = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                               ?? User.FindFirst("sub")?.Value
                               ?? User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (string.IsNullOrEmpty(userIdFromToken) || !Guid.TryParse(userIdFromToken, out var userId))
                throw new UnauthorizedAccessException("El ID de usuario no existe en el Token o su formato Guid es inválido.");

            return userId;
        }
    }
}
