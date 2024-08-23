using ClientApp.Models;
using Refit;

namespace ClientApp.RefitServices
{
    //[Headers("Content-Type: application/json")]
    public interface IRefitService
    {
        [Get("/WeatherForecast")]
        Task<IEnumerable<HavaDurumu>> HavaDurumunuGetir([Header("Authorization")] string bearerToken);
    }
}
