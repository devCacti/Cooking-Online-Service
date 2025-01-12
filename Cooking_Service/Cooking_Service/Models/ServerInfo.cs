using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace Cooking_Service.Models
{
    public class ServerInfo
    {
        public ServerInfo() { 
            GUID = Guid.NewGuid().ToString();
        }

        [Key, Required]
        public string GUID { get; set; }

        [Required, MaxLength(64)]
        public string ClientVersion { get; set; }

        [Required, MaxLength(64)]
        public string ServerVersion { get; set; }

        [Required, MaxLength(64)]
        public string LastSupportedClientVersion { get; set; }

        // Represents the date and time when this information was created
        // When reading this data, the servre itself should looking into the database and get the latest DateTimeCreated
        // Otherwise the server cannot correctly indicate whether the client is outdated or not.
        [Required]
        public DateTime DateTimeCreated { get; set; }


        // Location
        private static string _recipeImages = "C:/Cooking_Service/Uploads/Images/Recipes/";
        // The location of the recipe images
        public static string RecipeImages { get {
                CheckLocations();

                return _recipeImages;
            }
        }

        // Location checking
        public static void CheckLocations()
        {
            if (!System.IO.Directory.Exists(_recipeImages))
            {
                System.IO.Directory.CreateDirectory(_recipeImages);
            }
        }
    }
}