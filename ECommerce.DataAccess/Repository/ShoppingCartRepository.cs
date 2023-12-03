using ECommerce.DataAccess.Data;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {

        private readonly ApplicationDbContext _db;
        internal DbSet<ShoppingCart> dbSet;

        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
            this.dbSet = _db.Set<ShoppingCart>();
        }

        public ShoppingCart Get(Expression<Func<ShoppingCart, bool>> filter)
        {
            IQueryable<ShoppingCart> query = dbSet;
            query = query.Where(filter);
            return query.FirstOrDefault();

        }

        public IEnumerable<ShoppingCart> GetAll(Expression<Func<ShoppingCart, bool>> filter, string includeProperties = null)
        {
            IQueryable<ShoppingCart> query = dbSet;
            query = query.Where(filter);

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach(var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return query.ToList();
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }
    }
}
