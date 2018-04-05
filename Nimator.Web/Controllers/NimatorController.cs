using System;
using System.Threading.Tasks;
using System.Web.Http;
using Nimator.Logging;

#pragma warning disable 1998

namespace Nimator.Web.Controllers
{
    /// <summary>
    /// RPC-style controller to talk to the GlobalTicker (WIP)
    /// </summary>
    [RoutePrefix("api/nimator")]
    public sealed class NimatorController : ApiController
    {
        private readonly ILog _logger;

        public NimatorController(ILog logger)
        {
            _logger = logger;
        }

        [Route("start-ticking"), HttpGet]
        public async Task<IHttpActionResult> StartTicking()
        {
            try
            {
                HealthMonitor.StartTicking();
                _logger.Info($"[{nameof(NimatorController)}] GlobalTicker started");
                return Ok();
            }
            catch (Exception e)
            {
                _logger.ErrorException($"[{nameof(NimatorController)}] Failed to start the GlobalTicker", e);
                return InternalServerError();
            }
        }

        [Route("stop-ticking"), HttpGet]
        public async Task<IHttpActionResult> StopTicking()
        {
            try
            {
                HealthMonitor.StopTicking();
                _logger.Info($"[{nameof(NimatorController)}] GlobalTicker stopped");
                return Ok();
            }
            catch (Exception e)
            {
                _logger.ErrorException($"[{nameof(NimatorController)}] Failed to stop the GlobalTicker", e);
                return InternalServerError();
            };
        }
    }
}
