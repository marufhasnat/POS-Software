using POS.DataAccess.Data;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.DataAccess.Repository
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderRepository(ApplicationDbContext db) : base(db) 
        {
            _db = db;
        }

        public void Update(Order obj)
        {
            _db.Orders.Update(obj);
        }
    }
}
