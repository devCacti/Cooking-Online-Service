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

    public enum TypePreviledge
    {
        FullUnlock,
        _5Recipes,
        _10Recipes,
        _20Recipes,
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

        public virtual ICollection<User> User { get; set; }
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
        public bool IsPrivate { get; set; }

        [Required]
        public TypeUser Type { get; set; }


        // - User's linked - //

        // The recipes of the user
        public virtual ICollection<Recipe> Recipes { get; set; }

        //User Flags, this is used by the admins to pay attention to the actions of the user
        //Many to many relation
        public virtual ICollection<Flag> Flags { get; set; }

    }

    public class Previledge
    {
        [Key, Required, MaxLength(128)]
        public string GUID { get; set; }

        [Required]
        public TypePreviledge Type { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        [Required]
        public virtual User User { get; set; }
    }

    public class Recipe
    {
        [Key, Required, MaxLength(128)]
        public string GUID { get; set; }

        // The image has to be a string because it will be translated to base64
        [MaxLength(4096)]
        public string Image { get; set; }

        [Required, MaxLength(64)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        [MaxLength(1024)]
        public virtual ICollection<Ingredient> Ingredients { get; set; }

        public virtual ICollection<Steps> Steps { get; set; }

        // Small number with decimal places
        public double Time { get; set; }
        public double Portions { get; set; }

        public TypeRecipe Type { get; set; }

        public bool IsFavorite { get; set; }

        public virtual User User { get; set; }
    }

    public class Ingredient
    {
        [Key, Required, MaxLength(128)]
        public string GUID { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(64)]
        public string Type { get; set; }

        [MaxLength(64)]
        public string Amount { get; set; }

        public virtual Recipe Recipes { get; set; }
    }

    public class Steps
    {
        [Key, Required, MaxLength(128)]
        public string GUID { get; set; }

        [Required, MaxLength(64)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        public virtual Recipe Recipes { get; set; }
    }

    // Shopping list is a no go because of the amount of space
    // it would take in the database, as well as being too complex.
    // In the future, I might add it.
}