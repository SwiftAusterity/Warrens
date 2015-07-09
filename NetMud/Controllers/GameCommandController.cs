using System.Net;
using System.Web.Http;

namespace Controllers
{
    public class GameCommandController : ApiController
    {
        // GET: api/Default/5
        public string RenderCommand(string command)
        {
            return "value";
        }
    }
}