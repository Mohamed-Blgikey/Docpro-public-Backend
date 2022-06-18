using Docpro.BL.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Interface
{
    public interface IDynamicRep<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> match,string[] includes = null);
        Task<T> GetByIdAsync(Expression<Func<T, bool>> match, string[] includes = null);
        Task<PagedList<T>> GetPageniation(PagedParams pagedParams, Expression<Func<T, bool>> match, string[] includes = null);
        T GetById(int id);
        T Add(T item);
        T Edit(T item);
        T Delete(T item);

        void SaveAllAsync();
    }
}
