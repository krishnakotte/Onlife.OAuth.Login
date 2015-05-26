using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Onlife.OAuth.WebClient.Startup))]
namespace Onlife.OAuth.WebClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
