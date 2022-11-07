using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ePaperLive.Startup))]
namespace ePaperLive
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
