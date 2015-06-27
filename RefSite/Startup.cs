using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RefSite.Startup))]
namespace RefSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
