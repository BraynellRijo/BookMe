using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IQueryRepository<T>
    {
       IQueryable<T> GetAllAsync();
        Task<T> GetByIdAsync(Guid id);
    }
}
