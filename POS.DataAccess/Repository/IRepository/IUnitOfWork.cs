using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        ISupplierRepository Supplier { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IStoreRepository Store { get; }
        IProductRepository Product { get; }

        void Save();
    }
}
