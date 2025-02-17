using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cooking_Service.Models
{

    // Create recipe view model
    // The view model that is responsible to then create a new recipe
    // 
    public class CreateRecipeViewModel
    {
        // The image is attached as files in the request
        // And the server saves the image and attaches its name
        // to the recipe information
        //[Display(Name = "Image")]
        //[MaxLength(4096)]
        //public string Image { get; set; }

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

        // Custom ingredient measurements
        // The measurements are given using the id
        // of the ingredient
        [MaxLength(5000)]
        public string CustomIngM { get; set; }

        [MaxLength(2048)]
        public string IngrAmounts { get; set; }

        [MaxLength(4096)]
        public string Steps { get; set; }

        public double Time { get; set; }
        public double Portions { get; set; }

        public TypeRecipe Type { get; set; }

        public bool IsPublic { get; set; }
    }

    // Edit (Update) Recipe View Model
    // Much like the Create Recipe View Model, but including the GUID
    public class UpdateRecipeViewModel
    {
        [Required]
        [Display(Name = "GUID")]
        public string GUID { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [MaxLength(512)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        // All the ingredients, bridges, custom measurements and amounts
        // all need to be in the same order, so they can be matched
        // That has to be done in the app, or be given in a json format
        [MaxLength(4096)]
        public string IngredientIds { get; set; }

        //[MaxLength(4096)]
        //public string BridgeIds { get; set; }

        [MaxLength(2048)]
        public string CustomIngM { get; set; }

        [MaxLength(2048)]
        public string IngrAmounts { get; set; }


        // The steps are given in a string in a JSON format
        [MaxLength(4096)]
        public string Steps { get; set; }

        public double Time { get; set; }
        public double Portions { get; set; }
        public TypeRecipe Type { get; set; }
        public bool IsPublic { get; set; }
    }


    // New Ingredient Model
    // Used to create a new ingredient
    // The user provides the name, unit and tag of the ingredient
    // The tag is a GUID as it will be managed by the service
    // If the GUID doesn't correspond to any tag, it'll be null
    public class NewIngredientModel
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

    // Multiple New Ingredients Model
    // Used for the creation of multiple ingredients
    public class MultipleNewIngredientsModel
    {
        [Required]
        [Display(Name = "Names")]
        public string Names { get; set; }

        [Display(Name = "Units")]
        public string Units { get; set; }

        [Display(Name = "Tags")]
        public string Tags { get; set; }

    }

    // Assign verified status to an ingredient
    // Only works if the user is an admin
    public class IngVerificationViewModel
    {
        // The admin provides the GUID of the ingredient
        [Required]
        [Display(Name = "GUID")]
        public string GUID { get; set; }

        // Defines if the ingredient is verified
        [Required]
        [Display(Name = "Verified")]
        public bool Verified { get; set; }
    }
}