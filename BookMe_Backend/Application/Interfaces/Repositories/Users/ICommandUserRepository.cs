using Domain.Entities.Users;
using Domain.Enums.Users;

namespace Application.Interfaces.Repositories.Users
{
    public interface ICommandUserRepository : ICommandRepository<User>
    {
        Task UpdatePasswordAsync(Guid id, string newPassword);
        Task CreateRefreshToken(Guid id, string refreshToken, DateTime expiryDate);
        Task UpdateEmailVerification(Guid id, bool isVerified);
        Task AddRoleAsync(Guid userId, RolesType role);
        Task RevokeRefreshToken(Guid id);
    }
}
