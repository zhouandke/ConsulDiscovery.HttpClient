using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        [Route("{name}")]
        public string Get(string name)
        {
            return $"Hello {name}";
        }
    }
}
