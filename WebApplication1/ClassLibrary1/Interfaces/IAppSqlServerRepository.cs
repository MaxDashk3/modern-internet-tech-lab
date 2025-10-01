using ClassLibrary1.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Interfaces
{
    public interface IAppSqlServerRepository : IRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
    }
}
