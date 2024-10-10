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
        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        [Required]
        public FlagType Type { get; set; }

        [MaxLength(128)]
        public string Description { get; set; }

        public virtual User User { get; set; }
    }

    public class User
    {
        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(128)]
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
        public Recipe()
        {
            GUID = Guid.NewGuid().ToString();
        }

        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        // The image has to be a string because it will be translated to base64
        [MaxLength(4096)]
        public string Image { get; set; }

        [Required, MaxLength(80)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        // The ingredients of the recipe
        public virtual ICollection<IngredientBridge> Bridges { get; set; }

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
        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        public virtual ICollection<Ingredient> Ingredients { get; set; }

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
        public IngredientBridge()
        {
            GUID = Guid.NewGuid().ToString();
        }

        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        [Required]
        public double Amount { get; set; }

        // The custom unit of the ingredient, like a cup, a spoon, etc.
        // If the user thinks that the ingredient is not well represented by the unit
        // they can add a custom unit.
        [MaxLength(16)]
        public string CustomUnit { get; set; }

        // One to many relation with the recipe and the ingredient
        // One Bridge can only have one recipe and one ingredient
        // Yet one recipe can have many bridges
        // And so can one ingredient
        [Required]
        public virtual Recipe Recipe { get; set; }

        [Required]
        public virtual Ingredient Ingredient { get; set; }
    }

    public class Ingredient
    {
        public Ingredient()
        {
            GUID = Guid.NewGuid().ToString();
            isVerified = false;
        }

        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        // The measuring unit of the ingredient, like grams, liters, etc.
        [MaxLength(16)]
        public string Unit { get; set; }

        // The type of the ingredient
        public IngTag Tag { get; set; }

        // Verified, without this, only admins can see the ingredient or the recipes that use it
        // For example, if a user needs a new ingredient, they can add it, but it will not be public
        // until an admin verifies it, as it might be a duplicate or a fake ingredient.
        public bool isVerified { get; set; }

        // The user that created the ingredient
        public virtual User Author { get; set; }

        // Many to many relation
        public virtual ICollection<IngredientBridge> Bridges { get; set; }
    }

    // Shopping list is a no go because of the amount of space
    // it would take in the database, as well as being too complex.
    // In the future, I might add it.
}
