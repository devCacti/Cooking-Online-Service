using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Cooking_Service.Startup))]
namespace Cooking_Service
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
