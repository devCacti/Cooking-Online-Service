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
        Cook
    }

    public enum TypeRecipe
    {
        Geral,
        Bolos,
        Tartes,
        Sobremesas,
        Pratos,
    }

    public class UserInfo
    {
        [Key]
        [Required]
        [MaxLength(128)]
        public string GUID { get; set; }

        [Required]
        [MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(64)]
        public string Surname { get; set; }

        public TypeUser Type { get; set; }

        public virtual ICollection<Recipe> Recipes { get; set; }

    }

    public class Recipe
    {
        [Key]
        [Required]
        [MaxLength(128)]
        public string GUID { get; set; }

        // The image has to be a string because it will be translated to base64
        [MaxLength(2048)]
        public string Image { get; set; }

        [Required]
        [MaxLength(64)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        public string Ingredients { get; set; }

        public string Steps { get; set; }

        public double Portions { get; set; }

        public double Time { get; set; }

        public TypeRecipe Type { get; set; }

        public bool isFavorite { get; set; }

        public virtual UserInfo User { get; set; }
    }
}