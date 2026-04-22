using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Users;
using Application.Interfaces.Repositories.Users;
using Microsoft.AspNetCore.Identity;
using Domain.Enums.Users;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepository : ICommandUserRepository, IQueryUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext context)
        {
            _dbContext = context;
        }

        public IQueryable<User> GetAllAsync()
        {
            var users = _dbContext.Users.AsNoTracking();
            return users;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task CreateAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateAsync(Guid id, User user)
        {
            await _dbContext.Users
                   .Where(u => u.Id == id)
                   .ExecuteUpdateAsync(s => s
                   .SetProperty(u => u.Email, user.Email)
                   .SetProperty(u => u.HashedPassword, user.HashedPassword)
                   .SetProperty(u => u.IsRemoved, user.IsRemoved)
                   );
        }

        public async Task DeleteAsync(Guid id)
        {
            await _dbContext.Users
                .Where(u => u.Id == id)
                .ExecuteUpdateAsync(setter => setter.SetProperty(u => u.IsRemoved, true));
        }

        public async Task CreateRefreshToken(Guid id, string refreshToken, DateTime expiryDate)
        {
            await _dbContext.Users
                .ExecuteUpdateAsync(u =>
                u.SetProperty(user => user.RefreshToken, refreshToken)
                .SetProperty(user => user.RefreshTokenExpiration, expiryDate)
                );
        }

        public async Task RevokeRefreshToken(Guid id)
        {
            await _dbContext.Users
                .Where(u => u.Id == id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(user => user.RefreshToken, (string?)null)
                    .SetProperty(user => user.RefreshTokenExpiration, (DateTime?)null)
                );
        }
        public async Task UpdatePasswordAsync(Guid id, string newPassword)
        {
            await _dbContext.Users
                   .Where(u => u.Id == id)
                   .ExecuteUpdateAsync(s => s
                   .SetProperty(u => u.HashedPassword, newPassword));
        }

        public async Task AddRoleAsync(Guid id, RolesType role)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id)
                    ?? throw new KeyNotFoundException("User not found.");

            if (!user.Roles.Contains(role))
            {
                user.Roles.Add(role);
                _dbContext.Entry(user).Property(u => u.Roles).IsModified = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateEmailVerification(Guid id, bool isVerified)
        {
            await _dbContext.Users
                .Where(u => u.Id == id)
                .ExecuteUpdateAsync(setter => setter.SetProperty(u => u.IsVerified, isVerified));
        }
    }
}
