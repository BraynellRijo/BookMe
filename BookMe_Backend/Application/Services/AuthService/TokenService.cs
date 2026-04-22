using Application.DTOs.TokensDTOs;
using Application.Interfaces.Repositories.Users;
using Application.Interfaces.Services.AuthServices;
using Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services.AuthService
{
    public class TokenService(IConfiguration configuration,
        ICommandUserRepository commandUserRepository,
        IQueryUserRepository queryUserRepository) : ITokenService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ICommandUserRepository _commandUserRepository = commandUserRepository;
        private readonly IQueryUserRepository _queryUserRepository = queryUserRepository;
        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim> {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                    };

            if(user.Roles.Any() && user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                }
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                _configuration.GetValue<string>("jwt:Key")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("jwt:Issuer"),
                audience: _configuration.GetValue<string>("jwt:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
                );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            return token;
        }

        public async Task<string> CreateRefreshToken(User user)
        {
            var refreshToken = GenerateRefreshToken();
            var expiryDate = DateTime.UtcNow.AddDays(7);

            await _commandUserRepository.CreateRefreshToken(user.Id, refreshToken, expiryDate);
            return refreshToken;
        }

        public async Task RevokeSessionAsync(Guid userId)
        {
            if(userId == Guid.Empty) 
                throw new ArgumentNullException(nameof(userId));

            await _commandUserRepository.RevokeRefreshToken(userId);
        }
        public async Task<TokenResponseDTO> CreateTokenResponse(User? user)
        {
            return new TokenResponseDTO
            {
                Accesstoken = GenerateAccessToken(user),
                RefreshToken = await CreateRefreshToken(user)
            };
        }

        public async Task<User?> ValidateRefreshTokenAsync(Guid idUser, string refreshToken)
        {
            var user = await _queryUserRepository.GetByIdAsync(idUser);
            if (user is null || user.RefreshToken != refreshToken)
                return null;

            return user;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var random = RandomNumberGenerator.Create();
            random.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
    }
}
