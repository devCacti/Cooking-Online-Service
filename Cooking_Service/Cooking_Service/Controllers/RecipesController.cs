using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cooking_Service.Controllers
{
    public class RecipesController : Controller
    {
        // GET: Recipes
        public ActionResult Index()
        {
            return View();
        }

        // PUT: Recipes/NewRecipe
        [HttpPut]
        [AllowAnonymous]
        public ActionResult NewRecipe()
        {
            return View();
        }

    }
}