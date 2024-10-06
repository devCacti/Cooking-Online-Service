using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cooking_Service.Models
{
    public class CreateRecipeViewModel
    {
        [Display(Name = "Image")]
        [MaxLength(4096)]
        public string Image { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [MaxLength(512)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [MaxLength(4096)]
        public string Ingredients { get; set; }

        [MaxLength(2048)]
        public string IngrTypes { get; set; }

        [MaxLength(2048)]
        public string IngrAmount { get; set; }

        [MaxLength(4096)]
        public string Steps { get; set; }

        public double Time { get; set; }
        public double Portions { get; set; }

        public TypeRecipe Type { get; set; }

        public bool IsPublic { get; set; }
    }
}