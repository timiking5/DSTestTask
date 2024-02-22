using Models;

namespace DSTestTask.Services.WeatherExcelParser;

public interface IWeatherParser
{
    Task<List<WeatherForecastEntry>> ParseExcelForecast(string file);
}
