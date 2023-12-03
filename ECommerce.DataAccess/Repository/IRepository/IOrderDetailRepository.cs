using ECommerce.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {

        IEnumerable<OrderDetail> GetAll(Expression<Func<OrderDetail, bool>> filter, string includeProperties = null);

        void Update(OrderDetail obj);

        void Save();
    }
}
