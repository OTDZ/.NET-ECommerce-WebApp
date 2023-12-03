using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository.IRepository
{
    // Generic Repository
    public interface IRepository<T> where T : class
    {
        // CRUD Operations

        IEnumerable<T> GetAll();

        T Get(int id);

        void Add(T obj);

        void Remove(T obj);

    }
}
