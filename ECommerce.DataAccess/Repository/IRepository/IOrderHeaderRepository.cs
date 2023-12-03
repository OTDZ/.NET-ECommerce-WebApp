using ECommerce.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);

        void UpdateStatus(int id, string orderStatus, string paymentStatus = null);

        void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);

        OrderHeader Get(Expression<Func<OrderHeader, bool>> filter, string includeProperties = null); 

		IEnumerable<OrderHeader> GetAll(Expression<Func<OrderHeader, bool>> filter, string includeProperties = null);

		void Save();
    }
}
