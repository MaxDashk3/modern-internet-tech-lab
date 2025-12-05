using ClassLibrary1.DataModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class GameViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Title")]
        [Required(ErrorMessage = "The {0} field is required.")]
        [Remote(action: "IsTitleUniqueForDeveloper", controller: "Games", HttpMethod = "POST", AdditionalFields = "DeveloperId,Id", ErrorMessage = "A game with this title already exists for the selected developer.")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(50)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "ImageUrl")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Year")]
        [Range(1800, 2100, ErrorMessage = "Year has to be within 1800-2100 range.")]
        public int Year { get; set; }

        [Display(Name = "Price")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Display(Name = "DeveloperId")]
        [Required(ErrorMessage = "The {0} field is required.")]
        public int DeveloperId { get; set; }

        public string? DeveloperTitle { get; set; }
    }
}
