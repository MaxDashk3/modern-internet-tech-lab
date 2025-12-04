using ClassLibrary1.DataModels;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class DeveloperViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [StringLength(70, ErrorMessage = "Length should be under {0} symbols.")]
        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Required(ErrorMessage = "The {0} field is required.")]
        [Display(Name = "ContactEmail")]
        [Remote(action: "IsEmailUnique", controller: "Developers", HttpMethod = "POST", AdditionalFields = nameof(Id), ErrorMessage = "Email already in use.")]
        public string ContactEmail { get; set; }

        [Display(Name = "ConfirmEmail")]
        [Compare("ContactEmail", ErrorMessage = "The field must match contact email.")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string ConfirmEmail { get; set; }
    }
}
