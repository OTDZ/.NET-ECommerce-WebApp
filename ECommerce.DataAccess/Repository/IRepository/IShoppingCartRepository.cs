using ECommerce.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        
        void Update(ShoppingCart obj);

        void Save();
        
        ShoppingCart Get(Expression<Func<ShoppingCart, bool>> filter);

        IEnumerable<ShoppingCart> GetAll(Expression<Func<ShoppingCart, bool>> filter, string includeProperties = null);
    }
}
