using System.Threading.Tasks;
using System.Web.Http;

#pragma warning disable 1998

namespace Nimator.Web.Controllers
{
    /// <summary>
    /// Simple ping-pong controller to verify connectivity.
    /// </summary>
    [RoutePrefix("api/ping")]
    public sealed class PingController : ApiController
    {
        [Route(""), HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            return Ok("pong");
        }

        [Route(""), HttpPost]
        public async Task<IHttpActionResult> Post(string message)
        {
            return Ok($"You sent: {message}");
        }
    }
}
