using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace POS.DataAccess.Repository.IRepository
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        void Update(OrderItem obj);
        IList<OrderItem> GetList(Expression<Func<OrderItem, bool>> predicate);
    }
}
