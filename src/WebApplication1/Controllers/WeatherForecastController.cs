using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IHttpClientFactory httpClientFactory;

        public WeatherForecastController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var httpClient = httpClientFactory.CreateClient("SayHelloService");
            var result = await httpClient.GetStringAsync("Hello/NetCore");
            return $"WeatherForecast return:           {result}";
        }
    }
}
