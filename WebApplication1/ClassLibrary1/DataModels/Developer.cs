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
        [Required]
        public string Title { get; set; }
        [StringLength(70)]
        public string Description { get; set; }
        [EmailAddress]
        [Required]
        [Remote(action: "IsEmailUnique", controller: "Developers", HttpMethod = "POST", AdditionalFields = nameof(Id), ErrorMessage = "Email already in use")]
        public string ContactEmail { get; set; }
        [NotMapped]
        [Compare("ContactEmail")]
        [Required]
        public string ConfirmEmail { get; set; }
        public ICollection<Game> Games { get; } = new List<Game>();

    }
}
