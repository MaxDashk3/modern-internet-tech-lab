using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.DataModels
{
    public class User: IdentityUser
    {
        public ICollection<Game> Games { get; } = new List<Game>();
    }
}
