using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ApplicationUser obj)
        {
            _db.ApplicationUsers.Update(obj);
        }

        public IEnumerable<string> GetUserRoles(string userId)
        {
            var roleIds = _db.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToList();
            return _db.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToList();
        }

    }
}
