using Microsoft.AspNetCore.Mvc;

namespace ConsulDiscovery.HttpClient
{
    public class HealthCheckController : ControllerBase
    {
        [Route("/HealthCheck")]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("OK");
        }
    }
}
