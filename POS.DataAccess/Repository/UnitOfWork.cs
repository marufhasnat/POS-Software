using POS.DataAccess.Data;
using POS.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public ICategoryRepository Category { get; private set; }
        public ISupplierRepository Supplier { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IStoreRepository Store { get; private set; }

        public UnitOfWork(ApplicationDbContext db) 
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Supplier = new SupplierRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            Store = new StoreRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
