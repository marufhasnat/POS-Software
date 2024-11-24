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
    public class StoreRepository : Repository<Store>, IStoreRepository
    {
        private readonly ApplicationDbContext _db;
        public StoreRepository(ApplicationDbContext db) : base(db) 
        {
            _db = db;
        }

        public void Update(Store obj)
        {
            _db.Stores.Update(obj);
        }
    }
}
