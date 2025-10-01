using ClassLibrary1.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Repositories
{
    public class BaseSqlServerRepository<TdbContext> : IRepository
        where TdbContext : DbContext
    {

        protected TdbContext Db { get; set; }

        public BaseSqlServerRepository(TdbContext db)
        {
            Db = db;
        }

        public Task<int> AddAsync<T>(T item) where T : class
        {
            Db.Entry(item).State = EntityState.Added;

            return Db.SaveChangesAsync();
        }

        public IQueryable<T> All<T>() where T : class
        {
            return Db.Model.FindEntityType(typeof(T)) != null 
                ? Db.Set<T>().AsQueryable()
                : new List<T>().AsQueryable();
        }

        public async Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await All<T>().FirstOrDefaultAsync(expression);
        }

        public async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await All<T>().AnyAsync(expression);
        }

        public IQueryable<T> ReadAll<T>() where T : class
        {
            return All<T>().AsNoTracking();
        }

        public async Task<T?> ReadSingleAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await ReadAll<T>().SingleOrDefaultAsync(expression);
        }

        public IQueryable<T> ReadWhere<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return ReadAll<T>().Where(expression);
        }

        public async Task<int> RemoveAsync<T>(T item) where T : class
        {
            Db.Remove(item);
            return await Db.SaveChangesAsync();
        }

        public async Task<T?> SingleAsync<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return await All<T>().SingleOrDefaultAsync(expression);
        }

        public async Task<int> UpdateAsync<T>(T item) where T : class
        {
            var local = Db.Set<T>()
                .Local
                .FirstOrDefault(entry => entry == item);

            if (local != null)
            {
                Db.Entry(local).State = EntityState.Detached;
            }

            else
            {
                Db.Entry(item).State = EntityState.Modified;
            }

            Db.Update(item);
            return await Db.SaveChangesAsync();
        }
    }
}
