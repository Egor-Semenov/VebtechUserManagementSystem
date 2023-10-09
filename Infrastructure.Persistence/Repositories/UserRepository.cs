using Domain.Interfaces.Repositories;
using Domain.Models.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : IBaseRepository<User>
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task Create(User entity)
        {
            _dbContext.Set<User>().Add(entity);
            return _dbContext.SaveChangesAsync();
        }

        public Task Delete(User entity)
        {
            _dbContext.Set<User>().Remove(entity);
            return _dbContext.SaveChangesAsync();
        }

        public IQueryable<User> FindAll() =>
            _dbContext.Set<User>().Include(x => x.Roles);

        public IQueryable<User> FindByCondition(Expression<Func<User, bool>> expression) =>
            _dbContext.Set<User>()
            .Include(x => x.Roles)
            .Where(expression);

        public Task Update(User entity)
        {
            _dbContext.Set<User>().Update(entity);  
            return _dbContext.SaveChangesAsync();
        }
    }
}
