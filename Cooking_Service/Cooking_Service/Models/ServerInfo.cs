using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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
    }
}