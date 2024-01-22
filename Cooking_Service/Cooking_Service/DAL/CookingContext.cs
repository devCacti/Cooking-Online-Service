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
            Database.SetInitializer<CookingContext>(new DropCreateDatabaseIfModelChanges<CookingContext>());
        }

        public DbSet<UserInfo> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
    }

    public class CookingInitializer : DropCreateDatabaseIfModelChanges<CookingContext>
    {
        protected override void Seed(CookingContext context)
        {
            var users = new List<UserInfo>
            {
                new UserInfo { GUID = "6b547a86-c6ca-4ec0-b4a6-3e16baa22c74", Name = "Administrator", Surname = "Admin", Type = TypeUser.Admin },
                new UserInfo { GUID = Guid.NewGuid().ToString(), Name = "User", Surname = "User Type", Type = TypeUser.User },
                new UserInfo { GUID = Guid.NewGuid().ToString(), Name = "Cook", Surname = "Certified Cook", Type = TypeUser.Cook }
            };

            users.ForEach(s => context.Users.Add(s));
            context.SaveChanges();

            var recipes = new List<Recipe>
            { 
                new Recipe { GUID = Guid.NewGuid().ToString(), Title = "Bolo de Chocolate", Description = "Bolo de chocolate com cobertura de chocolate", Type = TypeRecipe.Bolos, User = users[0] },
                new Recipe { GUID = Guid.NewGuid().ToString(), Title = "Bolo de Bolacha", Description = "Bolo de bolacha com café", Type = TypeRecipe.Bolos, User = users[1] },
                new Recipe { GUID = Guid.NewGuid().ToString(), Title = "Tarte de Maçã", Description = "Tarte de maçã com canela", Type = TypeRecipe.Tartes, User = users[2] },
                new Recipe { GUID = Guid.NewGuid().ToString(), Title = "Tarte de Limão", Description = "Tarte de limão com merengue", Type = TypeRecipe.Tartes, User = users[0] },
                new Recipe { GUID = Guid.NewGuid().ToString(), Title = "Mousse de Chocolate", Description = "Mousse de chocolate com natas", Type = TypeRecipe.Sobremesas, User = users[1] },
                new Recipe { GUID = Guid.NewGuid().ToString(), Title = "Mousse de Manga", Description = "Mousse de manga com natas", Type = TypeRecipe.Sobremesas, User = users[2] },
                new Recipe { GUID = Guid.NewGuid().ToString(), Title = "Bacalhau com Natas"}
            };

            recipes.ForEach(s => context.Recipes.Add(s));
            context.SaveChanges();


        }
    }
}