using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.DataModels
{
    public class Developer
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ContactEmail { get; set; }
        public ICollection<Game> Games { get; } = new List<Game>();
        public string AuthorId { get; set; } = string.Empty;

    }
}
