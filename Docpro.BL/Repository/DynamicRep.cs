using Docpro.BL.Helper;
using Docpro.BL.Interface;
using Docpro.DAL.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Repository
{
    public class DynamicRep<T> : IDynamicRep<T> where T : class
    {
        #region fields
        private readonly AppDbContext appDb;

        #endregion
        public DynamicRep(AppDbContext appDb)
        {
            this.appDb = appDb;
        }



        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> match,string[] includes = null)
        {
            IQueryable<T> query = appDb.Set<T>();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return await query.Where(match).ToListAsync();
        }

        public T GetById(int id)
        {
            var data = appDb.Set<T>().Find(id);
            return data;
        }



        public async Task<T> GetByIdAsync(Expression<Func<T, bool>> match, string[] includes = null)
        {
            IQueryable<T> query =  appDb.Set<T>();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return await query.SingleOrDefaultAsync(match);
        }


        public T Add(T item)
        {
            appDb.Set<T>().Add(item);
            appDb.SaveChanges();
            return item;
        }

        public T Edit(T item)
        {
            appDb.Set<T>().Update(item);
            appDb.SaveChanges();
            return item;
        }

        public T Delete(T item)
        {
            appDb.Set<T>().Remove(item);
            appDb.SaveChanges();
            return item;
        }

        public void SaveAllAsync()
        {
            appDb.SaveChangesAsync();
        }

        public async Task<PagedList<T>> GetPageniation(PagedParams pagedParams,Expression<Func<T, bool>> match, string[] includes = null)
        {
            IQueryable<T> data = appDb.Set<T>();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    data = data.Include(include);
                }
            }
            data = data.Where(match);
            return await PagedList<T>.CreateAsync(data, pagedParams.PageNumber, pagedParams.PageSize);
        }
    }
}
