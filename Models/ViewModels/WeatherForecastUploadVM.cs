using Microsoft.AspNetCore.Http;

namespace Models.ViewModels;
public class WeatherForecastUploadVM
{
    public List<IFormFile> Files { get; set; }
}
