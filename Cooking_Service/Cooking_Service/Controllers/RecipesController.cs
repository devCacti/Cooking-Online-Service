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
                    // Checking for Ingredient IDS
                    var ingIDs = model.IngredientIds.Split(';');

                    // Checking for Ingredient Amounts
                    var ingAmounts = model.IngrAmounts.Split(';');

                    // Create a new recipe
                    var newRecipe = new Recipe
                    {
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

                    // Create the ingredient bridges
                    for (int i = 0; i < ingIDs.Length; i++)
                    {
                        // First check if it is possible to convert the amount to double, if not, just assign 0
                        try
                        {
                            Convert.ToDouble(ingAmounts[i]);
                        }
                        catch (Exception)
                        {
                            ingAmounts[i] = "0";
                        }

                        var newIngBridge = new IngredientBridge
                        {
                            Recipe = newRecipe,
                            Ingredient = db.Ingredients.FirstOrDefault(ing => ing.GUID == ingIDs[i]),
                            Amount = Convert.ToDouble(ingAmounts[i])
                        };

                        db.IngBridges.Add(newIngBridge);
                    }
                    db.SaveChanges();

                    // Return the GUID of the new recipe
                    return Json(new
                    {
                        message = "Recipe created successfully. Check recipe with id: " + newRecipe.GUID,
                        error = "",
                        code = "0"
                    }, JsonRequestBehavior.AllowGet);
                }
                else // If the model is not valid, return an error in JSON format
                {
                    return Json(new { error = "Invalid recipe data. Try again later", code = "2" }, JsonRequestBehavior.AllowGet);
                }
            }
            else // If the user is not authenticated, return an error in JSON format
            {
                return HttpNotFound();
            }
        }

        // GET: Recipes/GetRecipes
        [HttpGet]
        public ActionResult GetRecipes()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Get all public recipes
                var recipes = db.Recipes.Where(r => r.isPublic).Select(r => new
                {
                    GUID = r.GUID,
                    Image = r.Image,
                    Title = r.Title,
                    Description = r.Description,
                    Steps = r.Steps,
                    Time = r.Time,
                    Portions = r.Portions,
                    Type = r.Type,
                    isPublic = r.isPublic,
                    Author = UserManager.FindById(r.Author.GUID).UserName
                });

                return Json(new
                {
                    recipes = recipes,
                    error = "",
                    code = "0"
                }, JsonRequestBehavior.AllowGet);
            }
            else // If the user is not authenticated, return an error in JSON format
            {
                return HttpNotFound();
            }
        }


        // PUT: Recipes/NewIngredient
        [HttpPut]
        public ActionResult NewIngredient(NewIngredientViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    // Create a new ingredient
                    var newIngredient = new Ingredient
                    {
                        Name = model.Name,
                        Unit = model.Unit,
                        Tag = db.IngTags.FirstOrDefault(t => t.GUID == model.TagGUID) ?? null
                    };

                    db.Ingredients.Add(newIngredient);
                    db.SaveChanges();

                    // Return the name of the new ingredient
                    return Json(new
                    {
                        message = "Ingredient created successfully. Check ingredient with name: " + newIngredient.Name,
                        error = "",
                        code = "0"
                    }, JsonRequestBehavior.AllowGet);
                }
                else // If the model is not valid, return an error in JSON format
                {
                    return Json(new { error = "Invalid ingredient data. Try again later", code = "2" }, JsonRequestBehavior.AllowGet);
                }
            }
            else // If the user is not authenticated, return an error in JSON format
            {
                return HttpNotFound();
            }
        }

        // GET: Recipes/GetIngredients
        public ActionResult GetIngredients()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Get all verified ingredients but replace tag ID with tag name (if it exists)
                var ings = db.Ingredients.Where(ing => ing.isVerified).Select(ing => new
                {
                    GUID = ing.GUID,
                    Name = ing.Name,
                    Unit = ing.Unit,
                    Tag = ing.Tag != null ? ing.Tag.Name : "no_tag"
                });

                return Json(new
                {
                    ingredients = ings,
                    error = "",
                    code = "0"
                }, JsonRequestBehavior.AllowGet);
            }
            else // If the user is not authenticated, return an error in JSON format
            {
                return Json(new { error = "You must be logged in to see this information.", code = "1" }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Recipes/GetIngsNotVerified
        public ActionResult GetIngsNotVerified()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.Identity.IsAuthenticated && user.Type == TypeUser.Admin)
            {
                // Get all not verified ingredients
                var ings = db.Ingredients.Where(ing => !ing.isVerified);

                return Json(new
                {
                    ingredients = ings,
                    error = "",
                    code = "0"
                }, JsonRequestBehavior.AllowGet);
            }
            else // If the user is not authenticated, return an error in JSON format
            {

                // Trigger a 404 without throwing an exception
                return HttpNotFound();
            }
        }

        [HttpPut]
        public ActionResult SetIngVerified(IngVerificationViewModel model)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (User.Identity.IsAuthenticated && user.Type == TypeUser.Admin)
            {
                var ing = db.Ingredients.FirstOrDefault(i => i.GUID == model.GUID);
                if (ing != null)
                {
                    if (ing.isVerified == model.Verified)
                    {
                        return Json(new
                        {
                            message = "Ingredient verification status already set to " + model.Verified,
                            error = "",
                            code = "0"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    ing.isVerified = model.Verified;
                    int changes = db.SaveChanges();

                    return Json(new
                    {
                        message = "Ingredient verification status updated successfully -> Updated: " + changes + " row.",
                        error = "",
                        code = "0"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { error = "Ingredient not found", code = "2" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return HttpNotFound();
            }
        }

        // PUT: Recipes/NewTag
        [HttpPut]
        public ActionResult NewTag(string name)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    // Create a new tag
                    var newTag = IngTag.NewTag(name);

                    db.IngTags.Add(newTag);
                    db.SaveChanges();

                    // Return the name of the new tag
                    return Json(new
                    {
                        message = "Tag created successfully. Check tag with name: " + newTag.Name,
                        error = "",
                        code = "0"
                    }, JsonRequestBehavior.AllowGet);
                }
                else // If the model is not valid, return an error in JSON format
                {
                    return Json(new { error = "Invalid tag data. Try again later", code = "2" }, JsonRequestBehavior.AllowGet);
                }
            }
            else // If the user is not authenticated, return an error in JSON format
            {
                return HttpNotFound();
            }
        }
    }
}