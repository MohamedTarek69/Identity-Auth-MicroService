using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Domain.Contracts
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}
