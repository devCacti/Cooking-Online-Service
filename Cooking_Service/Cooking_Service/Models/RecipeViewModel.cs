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

        // There can be about 100 ingredients per recipe
        // 4096 / 36 ~= 113
        [MaxLength(4096)]
        public string IngredientIds { get; set; }

        [MaxLength(2048)]
        public string IngrAmounts { get; set; }

        [MaxLength(4096)]
        public string Steps { get; set; }

        public double Time { get; set; }
        public double Portions { get; set; }

        public TypeRecipe Type { get; set; }

        public bool IsPublic { get; set; }
    }

    public class NewIngredientViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [MaxLength(16)]
        [Display(Name = "Unit")]
        public string Unit { get; set; }

        [MaxLength(256)]
        [Display(Name = "Tag")]
        public string TagGUID { get; set; }
    }

    public class IngVerificationViewModel
    {
        [Required]
        [Display(Name = "GUID")]
        public string GUID { get; set; }

        [Required]
        [Display(Name = "Verified")]
        public bool Verified { get; set; }
    }
}