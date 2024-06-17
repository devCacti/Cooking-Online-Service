using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Cooking_Service.Models;
using Cooking_Service.DAL;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
namespace Cooking_Service.Controllers
{
    public class SearchViewModel
    {
        public string Type;
        public string GUID;
        public string UserName;
        public string Name;

        public SearchViewModel(string t,  string guid, string username, string name)
        {
            Type = t;
            GUID = guid;
            UserName = username;
            Name = name;
        }

        public SearchViewModel()
        {
        }
    }

    [Authorize]
    public class AdminController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private CookingContext Cooking_Context = new CookingContext();

        public AdminController()
        {
        }

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Admin
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Manage");
            }

            // This gets the type of the user of the current ID
            User user_contents = Cooking_Context.Users.First(u => u.GUID  == userId);

            // If it's not an adiminstrator, return to home page
            if (user_contents.Type != TypeUser.Admin)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }



        // GET: Handle a AJAX request, the request will contain:
        // - Type of request (Users, Recipes, Other)
        // - The page number
        // - The search query

        // The request will return a JSON object with all the results,
        // meaning that if there is more than one result, the JSON object
        // should contain a list of objects

        [HttpGet]
        public ActionResult GetResult(string type, int page, string search)
        {
            var uId = User.Identity.GetUserId();
            if (uId == null)
            {
                //Return JSON object with error
                return Json(new { error = "User not logged in." }, JsonRequestBehavior.AllowGet);
            }
            var cUser = Cooking_Context.Users.First(u => u.GUID == uId);
            if (cUser.Type != TypeUser.Admin)
            {
                //Return JSON object with error
                return Json(new { error = "403 - Forbidden - User is not an Admin" }, JsonRequestBehavior.AllowGet);
            }




            // Get the Type of Request, the Page Number and the Search Query
            string  r_type = type;      //Request Type
            int     r_page = page;      //Request Page
            string  r_search = search;  //Request Search

            // Type is User
            if (r_type == "Users")
            {
                // Get all the users
                var users = Cooking_Context.Users.ToList();
                var users_info = UserManager.Users.ToList();

                var f_users = new List<SearchViewModel>();

                // If there is a search query
                if (r_search != "")
                {
                    // Filter the users by the search query
                    try
                    {
                        users = users.Where(u => 
                                            u.Name.ToLower().Contains(r_search.ToLower()) ||
                                            u.GUID.Equals(r_search) ||
                                            u.Type.ToString().Equals(r_search, StringComparison.OrdinalIgnoreCase))
                                .OrderBy(u => u.Name).ToList() ?? users.ToList();
                        try
                        {
                            var t_user = users_info.Where(u => u.UserName.ToLower().Contains(r_search.ToLower()));
                            var t_user2 = users.Where(u => t_user.Any(t => t.Id == u.GUID)).ToList();
                            foreach (var user in t_user2)
                            {
                                if (!users.Contains(user))
                                {
                                    users.Add(user);
                                }
                            }
                        } catch { }
                    }
                    catch
                    {
                        //Return JSON object with error
                        return Json(new { error = "No Matches Found" }, JsonRequestBehavior.AllowGet);
                    }
                    foreach (var user in users)
                    {
                        // Temporary User Instance
                        var t_user = users_info.First(u => u.Id == user.GUID);
                        if (t_user != null)
                        {
                            f_users.Add(
                                new SearchViewModel(
                                    "User",
                                    user.GUID,
                                    t_user.UserName,
                                    user.Name
                                )
                            );
                        }
                    }
                }
                else
                {
                    users = users.ToList();
                }

                // Get the total number of users
                int total = users.Count();

                // Get the number of pages
                int pages = (int)Math.Ceiling((double)total / 10);

                // Get the users for the current page
                if (total >= 10)
                {
                    users = users.Skip((r_page - 1) * 10).Take(10).ToList();
                }

                // Return the JSON object with the users
                return Json(new { users = f_users, total = total, pages = pages }, JsonRequestBehavior.AllowGet);
            }
            if (r_type == "Recipes")
            {
                // Get all the recipes
                var recipes = Cooking_Context.Recipes.ToList();
                var users_info = UserManager.Users.ToList();

                var f_recipes = new List<SearchViewModel>();

                // If there is a search query
                if (r_search != "")
                {
                    // Filter the recipes by the search query
                    recipes = recipes.Where(r => r.Title.Contains(r_search) || r.GUID.Equals(r_search)).OrderBy(r => r.Title).ToList();
                    foreach (var recipe in recipes)
                    {
                        // Temporary User Instance
                        var t_user = users_info.First(u => u.Id == recipe.GUID);
                        if (recipes.Count() == users_info.Count())
                        {
                            f_recipes.Add(
                                new SearchViewModel(
                                    "Recipe",
                                    recipe.GUID,
                                    t_user.UserName,
                                    recipe.Title
                                )
                            );
                        }
                    }
                }
                else
                {
                    recipes = recipes.ToList();
                }

                // Get the total number of recipes
                int total = recipes.Count();

                // Get the number of pages
                int pages = (int)Math.Ceiling((double)total / 10);

                // Get the recipes for the current page
                recipes = recipes.Skip((r_page - 1) * 10).Take(10).ToList();

                // Return the JSON object with the recipes
                return Json(new { recipes = f_recipes, total = total, pages = pages }, JsonRequestBehavior.AllowGet);
            }

            if (r_type == "Other")
            {
                // Check for all
                /*
                 * Users
                 * User Flags
                 * Recipes
                */

                // Get all the users
                var users = Cooking_Context.Users.ToList();
                var users_info = UserManager.Users.ToList();

                // Get all the recipes
                var recipes = Cooking_Context.Recipes.ToList();

                // Get all the flags
                var flags = Cooking_Context.Flags.ToList();

                var f_results = new List<SearchViewModel>();


                // If there is a search query
                if (r_search != "")
                {
                    // Filter the users by the search query
                    users = users.Where(u => u.Name.Contains(r_search) || u.GUID.Equals(r_search) || u.Type.Equals(r_search)).OrderBy(u => u.Name).ToList();
                    foreach (var user in users)
                    {
                        // Temporary User Instance
                        var t_user = users_info.First(u => u.Id == user.GUID);
                        if (users.Count() == users_info.Count())
                        {
                            f_results.Add(
                            new SearchViewModel(
                                "User",
                                user.GUID,
                                t_user.UserName,
                                user.Name
                            ));
                        }
                    }

                    // Filter the recipes by the search query
                    recipes = recipes.Where(r => r.Title.Contains(r_search) || r.GUID.Equals(r_search)).OrderBy(r => r.Title).ToList();
                    foreach (var recipe in recipes)
                    {
                        // Temporary User Instance
                        var t_user = users_info.First(u => u.Id == recipe.GUID);
                        if (recipes.Count() == users_info.Count())
                        {
                            f_results.Add(
                            new SearchViewModel(
                                "Recipe",
                                recipe.GUID,
                                t_user.UserName,
                                recipe.Title
                            ));
                        }
                    }

                    // Filter the flags by the search query
                    flags = flags.Where(f => f.GUID.Equals(r_search) || f.Description.Contains(r_search)).OrderBy(f => f.Description).ToList();
                    foreach (var flag in flags)
                    {
                        // Temporary User Instance
                        var t_user = users_info.First(u => u.Id == flag.GUID);
                        if (flags.Count() == users_info.Count())
                        {
                            f_results.Add(
                            new SearchViewModel(
                                "Flag",
                                flag.GUID,
                                t_user.UserName,
                                flag.Description.Length > 10 ? flag.Description.Substring(0, 10) : flag.Description
                            ));
                        }
                    }

                    // Organize the results
                    //f_results = f_results.OrderBy(r => r.Type).ToList(); // This is not needed as the results are already ordered

                    // Get the total number of results
                    int total = f_results.Count();

                    // Get the number of pages
                    int pages = (int)Math.Ceiling((double)total / 10);

                    // Get the results for the current page
                    f_results = f_results.Skip((r_page - 1) * 10).Take(10).ToList();

                    // Return the JSON object with the results
                    return Json(new { results = f_results, total = total, pages = pages }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // Lista todos os resultados
                    users = users.ToList();
                    recipes = recipes.ToList();
                    flags = flags.ToList();

                    f_results = new List<SearchViewModel>();

                    // Adds the users to the results
                    foreach (var user in users)
                    {
                        f_results.Add(
                            new SearchViewModel(
                            "User",
                            user.GUID,
                            users_info.First(u => u.Id == user.GUID).UserName,
                            user.Name
                        ));
                    }

                    // Adds the recipes to the results
                    foreach (var recipe in recipes)
                    {
                        f_results.Add(
                            new SearchViewModel(
                            "Recipe",
                            recipe.GUID,
                            users_info.First(u => u.Id == recipe.GUID).UserName,
                            recipe.Title
                        ));
                    }                    
                    
                    // Adds the flags to the results
                    foreach (var flag in flags)
                    {
                        f_results.Add(
                            new SearchViewModel(
                            "Flag",
                            flag.GUID,
                            users_info.First(u => u.Id == flag.User.GUID).UserName,
                            flag.Description.Length > 10 ? flag.Description.Substring(0, 10) : flag.Description
                        ));
                    }

                    // Organize the results
                    //f_results = f_results.OrderBy(r => r.Type).ToList(); // This is not needed as the results are already ordered

                    // Get the total number of results
                    int total = f_results.Count();

                    // Get the number of pages
                    int pages = (int)Math.Ceiling((double)total / 10);

                    // Get the results for the current page
                    f_results = f_results.Skip((r_page - 1) * 10).Take(10).ToList();

                    // Return the JSON object with the results
                    return Json(new { results = f_results, total = total, pages = pages }, JsonRequestBehavior.AllowGet);
                }
            }
            //Return JSON object with error
            return Json(new { error = "Invalid Request Type" }, JsonRequestBehavior.AllowGet);
        }
    }
}