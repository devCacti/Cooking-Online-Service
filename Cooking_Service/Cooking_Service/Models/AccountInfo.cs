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
        [Key, Required, MaxLength(128)]
        public string GUID { get; set; }

        // The image has to be a string because it will be translated to base64
        [MaxLength(2048)]
        public string Image { get; set; }

        [Required, MaxLength(64)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        // Saves a JSON string that is decoded before being shown
        // on the user interface, either the website or the app.
        [MaxLength(1024)]
        public string Ingredients { get; set; }
        [MaxLength(1024)]
        public string IngrTypes { get; set; }
        [MaxLength(1024)]
        public string IngrAmount { get; set; }
        [MaxLength(2048)]
        public string Steps { get; set; }

        // Small number with decimal places
        public double Time { get; set; }
        public double Portions { get; set; }

        public TypeRecipe Type { get; set; }

        public bool isFavorite { get; set; }

        public virtual User User { get; set; }
    }

    // Shopping list is a no go because of the amount of space
    // it would take in the database, as well as being too complex.
    // In the future, I might add it.
}