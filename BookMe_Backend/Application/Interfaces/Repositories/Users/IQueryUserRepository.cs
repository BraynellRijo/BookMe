using Domain.Entities.Users;

namespace Application.Interfaces.Repositories.Users
{
    public interface IQueryUserRepository : IQueryRepository<User>
    {
        Task<User> GetUserByEmail(string email);
    }
}
