using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(NetMud.Startup))]
namespace NetMud
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
