using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;


namespace Cooking_Service.DAL
{
    public class CookingContext : DbContext
    {
        public CookingContext() : base("CookingContext")
        {
            Database.SetInitializer<CookingContext>(new DropCreateDatabaseIfModelChanges<CookingContext>());
        }

        public DbSet<Models.UserInfo> Users { get; set; }
        public DbSet<Models.Recipe> Recipes { get; set; }
    }

    public class CookingInitializer : DropCreateDatabaseIfModelChanges<CookingContext>
    {
        protected override void Seed(CookingContext context)
        {
            var users = new List<Models.UserInfo>
            {
                new Models.UserInfo { GUID = Guid.NewGuid().ToString(), Name = "Admin", Surname = "Administrator", Type = Models.TypeUser.Admin },
                new Models.UserInfo { GUID = Guid.NewGuid().ToString(), Name = "User", Surname = "User Type", Type = Models.TypeUser.User },
                new Models.UserInfo { GUID = Guid.NewGuid().ToString(), Name = "Cook", Surname = "Certified Cook", Type = Models.TypeUser.Cook }
            };

            users.ForEach(s => context.Users.Add(s));
            context.SaveChanges();

            var recipes = new List<Models.Recipe>
            {
                new Models.Recipe { GUID = Guid.NewGuid().ToString(), Title = "Bolo de Chocolate", Description = "Bolo de chocolate com cobertura de chocolate", Type = Models.TypeRecipe.Bolos, User = users[0] },
                new Models.Recipe { GUID = Guid.NewGuid().ToString(), Title = "Bolo de Bolacha", Description = "Bolo de bolacha com café", Type = Models.TypeRecipe.Bolos, User = users[1] },
                new Models.Recipe { GUID = Guid.NewGuid().ToString(), Title = "Tarte de Maçã", Description = "Tarte de maçã com canela", Type = Models.TypeRecipe.Tartes, User = users[2] },
                new Models.Recipe { GUID = Guid.NewGuid().ToString(), Title = "Tarte de Limão", Description = "Tarte de limão com merengue", Type = Models.TypeRecipe.Tartes, User = users[0] },
                new Models.Recipe { GUID = Guid.NewGuid().ToString(), Title = "Mousse de Chocolate", Description = "Mousse de chocolate com natas", Type = Models.TypeRecipe.Sobremesas, User = users[1] },
                new Models.Recipe { GUID = Guid.NewGuid().ToString(), Title = "Mousse de Manga", Description = "Mousse de manga com natas", Type = Models.TypeRecipe.Sobremesas, User = users[2] },
                new Models.Recipe { GUID = Guid.NewGuid().ToString(), Title = "Bacalhau com"}
            };
        }
    }
}