using Microsoft.AspNetCore.Mvc;
using Vruchtgebruik.Api.Models;

namespace Vruchtgebruik.Api.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Returns basic health information for the API.
        /// </summary>
        /// <remarks>
        /// This is the preferred health check endpoint for use with Swagger and monitoring systems.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HealthStatusResponse))]
        public ActionResult<HealthStatusResponse> Get()
        {
            return Ok(new HealthStatusResponse());
        }
    }
}
