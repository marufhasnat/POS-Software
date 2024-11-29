using Microsoft.EntityFrameworkCore;
using POS.DataAccess.Data;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace POS.DataAccess.Repository
{
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderItemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderItem obj)
        {
            _db.OrdersItems.Update(obj);
        }

        public IList<OrderItem> GetList(Expression<Func<OrderItem, bool>> predicate)
        {
            return _db.OrdersItems.Where(predicate).ToList();
        }

    }
}
