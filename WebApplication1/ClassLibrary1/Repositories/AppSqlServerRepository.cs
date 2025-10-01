using ClassLibrary1.Data;
using ClassLibrary1.DataModels;
using ClassLibrary1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Repositories
{
    public class AppSqlServerRepository: BaseSqlServerRepository<ApplicationDbContext>, IAppSqlServerRepository
    {
        public AppSqlServerRepository(ApplicationDbContext db) : base(db)
        {
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await FirstOrDefaultAsync<User>(u => u.Email == email);
        }
    }
}
