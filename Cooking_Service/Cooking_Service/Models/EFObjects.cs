using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cooking_Service.DAL;

// EFObjects
// -----------------------
// Entity Framwork Objects
// -----------------------
// This is the file that contains the classes for the Cooking Context Database
// This file is used to create the tables in it

namespace Cooking_Service.Models
{
    public enum TypeUser
    {
        Admin,
        User,
        Base,
        Premium,
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
        public Flag()
        {
            GUID = Guid.NewGuid().ToString();
        }

        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        // Indicates what type of flag the user has
        [Required]
        public FlagType Type { get; set; }

        [MaxLength(128)]
        public string Description { get; set; }

        // 1. It's only one user per flag has each flag has its own
        // description
        // 2. When creating a new flag, the description is inserted
        // as an explenation to why the flag is being atributed
        // 3. Now, an user can have more than one flag
        public virtual User User { get; set; }
    }

    public class Limit
    {
        private CookingContext db;
        public Limit()
        {
            // Atributes a new GUID
            GUID = Guid.NewGuid().ToString();
        }

        // Global Unique Identifier
        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        // Description about the limit (Only admins can access)
        [MaxLength(128)]
        public string Description { get; set; }

        // Depends on the program that the user has
        public double ImageSize { get; set; }

        // Depends on the program that the user has
        public double TotalStorage { get; set; }

        // Which users this Limit has
        public virtual ICollection<User> Users { get; set; }

        // Static function to get the user types
        // This is entended to be used by an admin
        // In theory, no regular user should access this
        public static List<String> GetUserTypes()
        {
            // Defines a new List of strings
            List<String> types = new List<String>();

            // Runs through all the values of the TypeUser enum
            // Then attaches the string value of the enum to the list
            foreach (TypeUser type in Enum.GetValues(typeof(TypeUser)))
            {
                types.Add(type.ToString());
            }

            // Returns the list of strings
            return types;
        }

        public static Tuple<double, double> GetMyLimit(User user)
        {
            // This creates a new Tuple with two doubles
            // It's created with 0.0, 0.0 because if it fails to
            // get the limit of the user, there is something wrong
            Tuple<double,double> r = Tuple.Create(0.0, 0.0);

            // Look through the limit of the user
            if (user.Limit != null)
            {
                // If the user has a limit, then return it
                // By using Entity Framework, it's not needed
                // to trigger a new database request

                // We get the max image size and the total storage
                // that the user can use
                // and then we attach it to r (result)
                r = Tuple.Create(
                    user.Limit.ImageSize, 
                    user.Limit.TotalStorage
                );
            }
            else return null; // Returns null if there is no limit

            // Return the result
            return r;
        }
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

        // The ingredients of the user
        public virtual ICollection<Ingredient> Ingredients { get; set; }

        //User Flags, this is used by the admins to pay attention to the actions of the user
        //Many to many relation
        public virtual ICollection<Flag> Flags { get; set; }

        // User Limits, this is used to determine the space limit of the user
        // One to many relation
        // One Limit for many users
        public Limit Limit { get; set; }
    }

    public class Recipe
    {
        public Recipe()
        {
            GUID = Guid.NewGuid().ToString();
        }

        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        // The image has to be a string because it will be a directory
        [MaxLength(4096)]
        public string Image { get; set; }

        [Required, MaxLength(80)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        // The ingredients of the recipe
        public virtual ICollection<IngredientBridge> Bridges { get; set; }

        // The steps of the recipe will be saved as a json string
        // As weird as it sounds, it's the best way to save it
        // Removing the limit for now because it can be a problem
        // in the future
        //[MaxLength(4096)]
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
    ///
    /// - Replaced by the IngTag class -
    /// </summary>

    public class IngTag
    {
        public IngTag()
        {
            GUID = Guid.NewGuid().ToString();
        }

        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        public virtual ICollection<Ingredient> Ingredients { get; set; }
    }

    public class IngredientBridge
    {
        public IngredientBridge()
        {
            GUID = Guid.NewGuid().ToString();
        }

        [Key, Required, MaxLength(64)]
        public string GUID { get; set; }

        public double Amount { get; set; } = 0.0;

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
