using Identity_Auth_MicroService.Domain.Contracts;
using Identity_Auth_MicroService.Presistance.Data.DbContexts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Presistance.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ClinicIdentityDbContext _dbContext;
        private readonly Dictionary<Type, object> _repositories = [];
        public UnitOfWork(ClinicIdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);

            if (_repositories.TryGetValue(entityType, out var repo))
                return (IGenericRepository<TEntity>)repo;

            var newRepo = new GenericRepository<TEntity>(_dbContext);
            _repositories[entityType] = newRepo;
            return newRepo;
        }


        public Task<int> SaveChangesAsync()
        {
            return _dbContext.SaveChangesAsync();
        }
    }
}
