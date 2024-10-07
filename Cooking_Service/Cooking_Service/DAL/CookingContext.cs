using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Cooking_Service.Models;

namespace Cooking_Service.DAL
{
    public class CookingContext : DbContext
    {
        public CookingContext() : base("DefaultConnection")
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CookingContext>());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }

        // Ingredient Related
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<IngredientBridge> IngBridges { get; set; }
        public DbSet<IngTag> IngTags { get; set; }

        // Flag Related
        public DbSet<Flag> Flags { get; set; }
    }
}