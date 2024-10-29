using Cooking_Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Web.Services.Description;
using Cooking_Service.DAL;
using Cooking_Service.CSFunctions;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Ajax.Utilities;
using System.Net.Configuration;
using System.Web.Http.Controllers;

/// <summary>
/// Everytime the responses are changed, the app needs to be updated too to avoid incompatibility
/// The app is expecting a JSON object with specific json formats
/// And if that format is changed, the app will not be able to parse the data correctly and may cause some errors either when saving 
/// or when displaying the data.
/// 
/// To avoid this, a minimum client version is saved on the server, and if the client doesn't meet that requirement, the service will return an error
/// Triggering a request to update the app.
/// 
/// This is how it should be, as of now (2024-10-9): This is not a reality yet, but is requested that it is implented in the future.
/// 
/// </summary>


namespace Cooking_Service.Controllers
{
    public class RecipesController : Controller
    {
        // Identity Managers
        // ASP.NET Provided Managers
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        // Cooking Context database
        private CookingContext db = new CookingContext();
        // Client custom functions
        private Functions _cl = new Functions();

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
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            var files = Request.Files;

            if (iCVV.Item1 == 404)
                return HttpNotFound();
            // Returns Not Found if code is 404

            else if (iCVV.Item1 == 403)
                return new HttpStatusCodeResult(403, iCVV.Item2);
            // Returns Forbidden if code is 403

            // Continues if the client version is correct

            // If the client version is correct, then continue with the request
            if (User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    // The version verifier is important for things like this that can be changed and need
                    // to be in a specific way to work with the app
                    // Checking for Ingredient IDS
                    List<string> ingIDs = new List<string>();
                       
                    // Checking for Ingredient Amounts
                    List<string> ingAmounts = new List<string>();

                    // Checking for Custom measured ingredients
                    List<string> customIngs = new List<string>();

                    if (model.IngredientIds != null)
                    {
                        // Split the string into a list of strings
                        ingIDs = model.IngredientIds.Split(';').ToList();

                        // Split the string into a list of strings
                        if (model.IngrAmounts != null)
                        {
                            ingAmounts = model.IngrAmounts.Split(';').ToList();

                        }

                        // Split the string into a list of strings
                        if (model.CustomIngM != null)
                        {
                            // Split the string into a list of strings
                            customIngs = model.CustomIngM.Split(';').ToList();
                        }
                        
                    }

                    // Get user from the CookingContext
                    var user = db.Users.Find(User.Identity.GetUserId());

                    // Create a new recipe
                    var newRecipe = new Recipe
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Steps = model.Steps,
                        Time = model.Time,
                        Portions = model.Portions,
                        Type = model.Type,
                        isPublic = model.IsPublic,
                        Author = user
                    };

                    // The path where the image will be saved
                    // Includes the name of the image
                    var path = "";

                    // File type is like this as it is received from a form
                    HttpPostedFileBase file = null;

                    // Save the first image using the GUID of the recipe + 'main' as the name
                    try // Try to save the image
                    {
                        // Gets the first element of all the files sent
                        file = files[0];

                        // Get the type of the file
                        var fileType = file.ContentType.Split('/')[1];
                        
                        // Convert to MB (value / 1024^2)
                        var fileSize = file.ContentLength / 1048576;

                        // Check if the file is an image
                        if (fileType != "jpg" && fileType != "jpeg" && fileType != "png")
                        {
                            // If the file is not a known image format, return an error in JSON format
                            return Json(new { error = "Invalid image format. Try again later", code = "2" }, JsonRequestBehavior.AllowGet);
                        }

                        // Check if the file is too big
                        if (fileSize > _cl.getImageSizeLimit(user))
                        {
                            // If the file is too big, return an error in JSON format
                            return Json(new { error = "Image is too big. Max size is 5MB", code = "2" }, JsonRequestBehavior.AllowGet);
                        }

                        // Decide the name of the file
                        var fileName = newRecipe.GUID + "_main." + fileType;

                        // Decide where the file will be saved
                        path = ServerInfo.RecipeImages + fileName;

                        // Assign the path to the image field
                        newRecipe.Image = path;

                        // Save the recipe before the bridge
                        db.Recipes.Add(newRecipe);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        // If the image is not saved, then the image will be null
                        throw e;
                        //newRecipe.Image = null;
                    }

                    // Create the ingredient bridges
                    for (int i = 0; i < ingIDs.Count; i++)
                    {
                        try
                        {
                            // Try to convert the amount to double
                            Convert.ToDouble(ingAmounts[i]);
                        }
                        catch (Exception)
                        {
                            // If the amount fails to convert, set it to 0
                            ingAmounts[i] = "0";
                        }

                        double _ingamount = Convert.ToDouble(ingAmounts[i]);

                        // Create a new ingredient bridge
                        try
                        {
                            // Getting the current id from the list is crucial
                            // This is because LINQ cannot translate the value from the list
                            var _ingIdNow = ingIDs[i];

                            // Custom ingredient measurement list
                            string[] _ingCM = null;

                            // Custom measurement unit of the ingredient
                            string _cUnit = null;

                            // Run through the custom ingredient measurements
                            // and check if any corresponds to the current ingredient being analysed
                            for (int j = 0; j < customIngs.Count; j++)
                            {
                                var _cIngs = customIngs[j].Split(':');

                                // 1. By checking if the id from the custom ingredient measurement is the same as the current ingredient id
                                // it's not needed to check if the id from that list is empty;
                                // 
                                // 2. By checking if the unit is not null or empty, even if the id was correctly atributed, the unit
                                // will be set to the default unit of the ingredient, this way, we avoid errors
                                if (_cIngs[0] == _ingIdNow && 
                                    !string.IsNullOrEmpty(_cIngs[1]))
                                {
                                    _ingCM = customIngs[j].Split(':');
                                    _cUnit = _ingCM[1];
                                    break;
                                }
                            }

                            // The the ingredient with the GUID
                            var _ing = db.Ingredients.FirstOrDefault(ing => ing.GUID == _ingIdNow);

                            // Create a new ingredient bridge
                            var newIngBridge = new IngredientBridge
                            {
                                Recipe = newRecipe,
                                Ingredient = _ing,
                                Amount = _ingamount,
                                CustomUnit = _cUnit
                            };

                            db.IngBridges.Add(newIngBridge);
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            //throw e;
                            return Json(new { error = "Something went wrong. e:" + e , ids = ingIDs});
                        }
                    }

                    // Save the image after everything is saved to avoid saving a picture before the recipe and components
                    file.SaveAs(path);

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

        // GET: Recipes/RecipeImage
        public ActionResult RecipeImage(string id)
        {
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
                return HttpNotFound();

            else if (iCVV.Item1 == 403)
                return new HttpStatusCodeResult(403, iCVV.Item2);

            // Get the recipe with the GUID
            var recipe = db.Recipes.FirstOrDefault(r => r.GUID == id);

            // If the recipe is not found, return an error in JSON format
            if (recipe == null)
            {
                return Json(new { error = "Recipe not found", code = "2" }, JsonRequestBehavior.AllowGet);
            }

            // Get the image path
            var path = recipe.Image;

            // Return the image
            return File(path, "image/jpeg");
        }

        // GET: Recipes/GetRecipes
        public ActionResult GetRecipes()
        {
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
                return HttpNotFound();

            else if (iCVV.Item1 == 403)
                return new HttpStatusCodeResult(403, iCVV.Item2);

            if (User.Identity.IsAuthenticated)
            {

                // Get all public recipes
                var recipes = db.Recipes.Where(r => r.isPublic).Select(r => new
                {
                    GUID = r.GUID,
                    Title = r.Title,
                    Description = r.Description,
                    Time = r.Time,
                    Portions = r.Portions,
                    Type = r.Type,
                    Author = UserManager.FindById(r.Author.GUID).UserName
                });

                return Json(new
                {
                    recipes = recipes,
                    error = "",
                    code = "0"
                }, JsonRequestBehavior.AllowGet);
            }
            // Get all public recipes
            var _recipes = db.Recipes.Where(r => r.isPublic).Select(r => new
            {
                GUID = r.GUID,
                Title = r.Title,
                Description = r.Description,
                Author = UserManager.FindById(r.Author.GUID).UserName
            });

            return Json(new
            {
                recipes = _recipes,
                error = "",
                code = "0"
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Recipes/GetMyRecipes
        public ActionResult GetMyRecipes()
        {
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
                return HttpNotFound();

            else if (iCVV.Item1 == 403)
                return new HttpStatusCodeResult(403, iCVV.Item2);

            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.Find(User.Identity.GetUserId());

                // Get all recipes of the user
                var recipes = user.Recipes.Select(r => new
                {
                    GUID = r.GUID,
                    // The image is not sent as it is a string containing the directory of the image
                    // Instead, it's sent with a different method that only sends the image, avoiding
                    // long request durations, the get image function only requires the id of the recipe
                    Title = r.Title,
                    Description = r.Description,
                    // In the final version of the Get My Recipes, the ingredients will not be shown
                    // This is because when the user gets all the recipes, only the main info is needed
                    Ingredients = r.Bridges.Select(b => new
                    {
                        GUID = b.GUID,
                        IngGUID = b.Ingredient.GUID,
                        Name = b.Ingredient.Name,
                        Unit = b.Ingredient.Unit,
                        Amount = b.Amount,
                        CustomUnit = b.CustomUnit,
                    }),
                    // The same goes for the steps
                    Steps = r.Steps,

                    Time = r.Time,
                    Portions = r.Portions,
                    Type = r.Type,
                    Tags = r.Bridges.Select(b => b.Ingredient.Tag != null ? b.Ingredient.Tag.Name : "no_tag"),
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


        // This function is used to Delete a recipe by its GUID
        // Only the user that created the recipe can delete it
        // GET: Recipes/GetRecipeById
        public ActionResult DeleteRecipeById(string guid)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                var recipe = db.Recipes.FirstOrDefault(r => r.GUID == guid);

                if (recipe != null)
                {
                    // Only the owner or admins can delete a recipe
                    if (recipe.Author.GUID == user.GUID || user.Type == TypeUser.Admin)
                    {
                        db.Recipes.Remove(recipe);
                        db.SaveChanges();

                        return Json(new { message = "Recipe deleted successfully", error = "", code = "0" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { error = "You are not the author of this recipe", code = "2" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { error = "Recipe not found", code = "2" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return HttpNotFound();
            }
        }
        // When creating new ingredients, the app sends a put to this endpoint
        // so that when the user clicks save recipe, the app attaches all the
        // ingredient IDs to the recipe
        // PUT: Recipes/NewIngredient
        [HttpPut]
        public ActionResult NewIngredient(NewIngredientViewModel model)
        {
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
                return HttpNotFound();

            else if (iCVV.Item1 == 403)
                return new HttpStatusCodeResult(403, iCVV.Item2);

            var user = db.Users.Find(User.Identity.GetUserId());

            if (User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    // Check if there is already a ingredient with this name
                    var ing_n = user.Ingredients.Count(ing => ing.Name == model.Name);

                    // If the ingredient already exists, return an error in JSON format
                    if (ing_n != 0)
                    {
                        // Get the id of the already existant ingredient
                        var ing_id = user.Ingredients.FirstOrDefault(ing => ing.Name == model.Name).GUID;
                        return Json(new { error = "Ingredient already exists", ing_id = ing_id, code = "2" }, JsonRequestBehavior.AllowGet);
                    }

                    // Create a new ingredient
                    var newIngredient = new Ingredient
                    {
                        Name = model.Name,
                        Unit = model.Unit,
                        Tag = db.IngTags.FirstOrDefault(t => t.GUID == model.TagGUID) ?? null,
                        Author = user
                    };

                    db.Ingredients.Add(newIngredient);
                    db.SaveChanges();

                    // Return the name of the new ingredient and id
                    return Json(new
                    {
                        message = "Ingredient created successfully. Check ingredient with name: " + newIngredient.Name,
                        ing_id = newIngredient.GUID,
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

        // GET: Recipes/GetMyIngredients
        public ActionResult GetMyIngredients()
        {
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
                return HttpNotFound();

            else if (iCVV.Item1 == 403)
                return new HttpStatusCodeResult(403, iCVV.Item2);

            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.Find(User.Identity.GetUserId());

                // Without the select json is unable to serialise the object, as it enters a loop of references
                var ings = user.Ingredients.Select(i => new
                {
                    GUID = i.GUID,
                    Name = i.Name,
                    Unit = i.Unit,
                    Tag = i.Tag != null ? i.Tag.Name : "no_tag",
                    isVerified = i.isVerified
                }).ToList();

                // Create a JSON object with the ingredients
                // Everytime this is updated the app needs to be updated too to avoid incompatibility
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

        // GET: Recipes/GetIngredients
        public ActionResult GetIngredients()
        {
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
                return HttpNotFound();

            else if (iCVV.Item1 == 403)
                return new HttpStatusCodeResult(403, iCVV.Item2);

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

                // Create a JSON object with the ingredients
                // Everytime this is updated the app needs to be updated too to avoid incompatibility
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

        // Admin Only function
        // GET: Recipes/GetIngsNotVerified
        public ActionResult GetIngsNotVerified()
        {
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
                return HttpNotFound();

            else if (iCVV.Item1 == 403)
                return new HttpStatusCodeResult(403, iCVV.Item2);

            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.Identity.IsAuthenticated && user.Type == TypeUser.Admin)
            {
                // Get all unverified ingredients
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

        // Admin Only function
        // PUT: Recipes/SetIngVerified
        [HttpPut]
        public ActionResult SetIngVerified(IngVerificationViewModel model)
        {
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
                return HttpNotFound();

            else if (iCVV.Item1 == 403)
                return new HttpStatusCodeResult(403, iCVV.Item2);

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
            // First check if the client version is correct before proceeding
            var iCVV = _cl.isClientVersionValid(Request);
            if (iCVV.Item1 == 404)
                return HttpNotFound();

            else if (iCVV.Item1 == 403)
                return new HttpStatusCodeResult(403, iCVV.Item2);

            if (User.Identity.IsAuthenticated)
            {
                if (ModelState.IsValid)
                {
                    // Create a new tag
                    var newTag = new IngTag { Name = name };

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