using Cooking_Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Web.Services.Description;
using Cooking_Service.DAL;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace Cooking_Service.Controllers
{
    public class RecipesController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private CookingContext db = new CookingContext();

        public RecipesController()
        {
        }

        public RecipesController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Recipes
        public ActionResult Index()
        {
            return View();
        }

        // PUT: Recipes/NewRecipe
        [HttpPut]
        public ActionResult NewRecipe(CreateRecipeViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    // Create a new recipe
                    var newRecipe = new Recipe
                    {
                        GUID = Guid.NewGuid().ToString(),
                        Image = model.Image,
                        Title = model.Title,
                        Description = model.Description,
                        Steps = model.Steps,
                        Time = model.Time,
                        Portions = model.Portions,
                        Type = model.Type,
                        isPublic = model.IsPublic,
                        Author = db.Users.Find(User.Identity.GetUserId())
                    };

                    db.Recipes.Add(newRecipe);
                    db.SaveChanges();

                    // Return the GUID of the new recipe
                    return Json(new
                    {
                        message = "Recipe created successfully. Check recipe with id: " + newRecipe.GUID,
                        error = "",
                        code = "0"
                    });
                }
                else
                {
                    return Json(new { error = "Invalid recipe data. Try again later", code = "2" });
                }
            }
            else
            {
                return Json(new { error = "You must be logged in to create a recipe.", code = "1" });
            }
        }

    }
}