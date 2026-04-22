using Application.DTOs.AuthDTOs;
using Application.DTOs.TokensDTOs;
using Domain.Entities.Users;

namespace Application.Interfaces.Services.AuthServices
{
    public interface IAuthService
    {
        Task<TokenResponseDTO?> LoginAsync(LoginDTO request);
        Task<User?> RegisterAsync(RegisterDTO request);
        Task SendVerificationEmailAsync(string email);
        Task<bool> VerifyCode(VerifyEmailDTO emailDTO);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDTO request);
        Task LogoutAsync(Guid userId);
        Task<TokenResponseDTO?> RefreshTokenAsync(RequestRefreshTokenDTO request);
        Task<TokenResponseDTO> BecomeHostAsync(Guid userId);
    }
}
