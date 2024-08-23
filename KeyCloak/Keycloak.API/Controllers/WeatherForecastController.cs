using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Keycloak.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Don", "Sert", "So�uk", "Serin", "Il�k", "S�cak", "Yumu�ak", "�ok S�cak", "Bunalt�c�", "Kavurucu"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<HavaDurumu> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new HavaDurumu
            {
                Tarih = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                SicaklikC = Random.Shared.Next(-20, 55),
                Ozet = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
