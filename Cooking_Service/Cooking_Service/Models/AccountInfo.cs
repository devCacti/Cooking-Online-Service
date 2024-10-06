using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cooking_Service.Models
{
    public enum TypeUser
    {
        Admin,
        User,
        Cook,
        Guest
    }

    public enum TypeRecipe
    {
        Geral,
        Bolos,
        Tartes,
        Sobremesas,
        Pratos,
    }

    public enum FlagType
    {
        Ban,
        Racism,
        Terrorism,
        Cyberbullying
    }

    // Many to many relation
    public class Flag
    {
        [Key]
        [Required, MaxLength(128)]
        public string GUID { get; set; }

        [Required]
        public FlagType Type { get; set; }

        [MaxLength(128)]
        public string Description { get; set; }

        public virtual User User { get; set; }
    }

    public class User
    {
        [Key, Required, MaxLength(128)]
        public string GUID { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(64)]
        public string Surname { get; set; }

        //The bio is a small description of the user
        [MaxLength(256)]
        public string Bio { get; set; }

        //This makes it so that other users can, or not, see this user's profile.
        //However, they can still report. Admins can see either way.
        [Required]
        public bool isPrivate { get; set; }

        [Required]
        public TypeUser Type { get; set; }


        // - User's linked - //

        // The recipes of the user
        public virtual ICollection<Recipe> Recipes { get; set; }

        //User Flags, this is used by the admins to pay attention to the actions of the user
        //Many to many relation
        public virtual ICollection<Flag> Flags { get; set; }

    }

    public class Recipe
    {
        private string guid;
        [Key, Required, MaxLength(128)]
        public string GUID {
            get { return guid; }
            set {
                if (value == null || string.IsNullOrEmpty(value))
                {
                    value = Guid.NewGuid().ToString();
                }
                guid = value;
            } 
        }

        // The image has to be a string because it will be translated to base64
        [MaxLength(4096)]
        public string Image { get; set; }

        [Required, MaxLength(80)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        // The ingredients of the recipe
        public virtual ICollection<RecipeIngredient> Ingredients { get; set; }

        [MaxLength(4096)]
        public string Steps { get; set; }

        // Small number with decimal places
        public double Time { get; set; }
        public double Portions { get; set; }

        public TypeRecipe Type { get; set; }

        public bool isAllowed { get; set; }
        public bool isPublic { get; set; }


        // Foreign key
        public virtual User Author { get; set; }
    }

    /// <summary>
    /// Unsued enumeration
    ///public enum IngrTag
    ///{
    ///    Meat,
    ///    Fish,
    ///    Vegetable,
    ///    Fruit,
    ///    Dairy,
    ///    Grain,
    ///    Spice,
    ///    Herb,
    ///    Other
    ///}
    /// </summary>

    public class IngTag
    {
        [Key, Required, MaxLength(128)]
        public string GUID { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        public virtual ICollection<RecipeIngredient> Ingredients { get; set; }

        public static IngTag NewTag(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return new IngTag()
            {
                GUID = Guid.NewGuid().ToString(),
                Name = name
            };
        }
    }

    public class IngredientBridge
    {
        [Key]
        public int GUID { get; set; }

        public virtual Recipe Recipe { get; set; }
        public virtual RecipeIngredient Ingredient { get; set; }
    }

    public class RecipeIngredient
    {
        [Key, Required, MaxLength(128)]
        public string GUID { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        // The amount of the ingredient, related to the unit if this value is 1.3 and the unit is grams, it means 1.3 grams
        [MaxLength(64)]
        public double Amount { get; set; }

        // The measuring unit of the ingredient, like grams, liters, etc.
        [MaxLength(16)]
        public string Unit { get; set; }

        // The type of the ingredient
        public IngTag Tag { get; set; }


        // Many to many relation
        public virtual ICollection<Recipe> Recipes { get; set; }
    }

    // Shopping list is a no go because of the amount of space
    // it would take in the database, as well as being too complex.
    // In the future, I might add it.
}
