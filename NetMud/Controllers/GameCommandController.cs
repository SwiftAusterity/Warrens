using System.Net;
using System.Web.Http;

using NetMud.Interp;

namespace Controllers
{
    public class GameCommandController : ApiController
    {
        [HttpGet]
        [AllowAnonymous]//for testing
        // GET: api/Default/5
        public string RenderCommand(string command)
        {
            return Interpret.Render(command, null);
        }
    }
}