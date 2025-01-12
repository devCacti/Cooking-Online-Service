using Cooking_Service.DAL;
using Cooking_Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cooking_Service.CSFunctions
{
    public class Functions
    {
        private CookingContext db = new CookingContext();

        public float getImageSizeLimit(User user)
        {
            // This function returns the image size limit for the user,
            // based on the user type
            // The user type is defined in the User class

            // The default image size limit is 2.5MB
            float imageSizeLimit = 2.5f;

            return imageSizeLimit;
        }

        public Tuple<int, string> isClientVersionValid(HttpRequestBase Request)
        {
            bool bypass = true;
            if (bypass)
            {
                return Tuple.Create(200, "Bypassed. In Testing.");
            }

            var clientVersion = Request.Headers["client-version"];

            // Second check if the client version matches any of the registered versions on the db
            var clientVersionChecked = db.ServerInfo.First(s => s.ClientVersion == clientVersion);

            if (clientVersionChecked == null)
            {
                // If the client version is not found, then the client is fake, its not the app
                // To not indicate that the version is wrong, just say that there is no such page or directory
                return Tuple.Create(404, "Client version not found");
            }
            else
            {
                // Check when the version was registered, if it is older than the last supported version, then the client is outdated
                // Get the last supported version
                var lastVersion = db.ServerInfo.OrderByDescending(s => s.DateTimeCreated).First();
                var latestSupportedVersion = db.ServerInfo.First(s => s.ClientVersion == lastVersion.LastSupportedClientVersion);

                // If the client version is older than the last supported version, then the client is outdated
                if (clientVersionChecked.DateTimeCreated < latestSupportedVersion.DateTimeCreated)
                {
                    // Tell the client that the version is outdated and needs an update
                    return Tuple.Create(403, "update_request");
                }
                else
                {
                    return Tuple.Create(200, "Client version is valid");
                }
            }
        }
    }
}