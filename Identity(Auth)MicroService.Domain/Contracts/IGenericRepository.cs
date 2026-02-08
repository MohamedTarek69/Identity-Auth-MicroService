using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Domain.Contracts
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(object id);
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Remove(TEntity entity);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
