using Application.DTOs.TokensDTOs;
using Domain.Entities.Users;

namespace Application.Interfaces.Services.AuthServices
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        Task<string> CreateRefreshToken(User user);
        Task RevokeSessionAsync(Guid userId);
        Task<TokenResponseDTO> CreateTokenResponse(User? user);
        Task<User?> ValidateRefreshTokenAsync(Guid idUser, string refreshToken);    
    }
}
