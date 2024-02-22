using Models;
using OfficeOpenXml;
using System.Data;

namespace DSTestTask.Services.WeatherExcelParser;

public class WeatherParser : IWeatherParser
{
    private readonly ILogger<IWeatherParser> _logger;

    public WeatherParser(ILogger<IWeatherParser> logger)
    {
        _logger = logger;
    }
    /// <summary>
    /// Можно было бы организовать поиск по полям заголовков, но мне это показалось чересчур сложным.
    /// В таком случае нет никакой гарантии, что мы найдём нужный заголовок, т.к. он может называться,
    /// например, не Т, а Темпа. Для тестового задания достаточно будет такой реализации.
    /// </summary>
    private const int dateCol = 1;
    private const int tempCol = 3;
    private const int humidityCol = 4;
    private const int airPressureCol = 6;
    private const int windSpeedCol = 8;
    private const int cloudinessCol = 9;
    public async Task<List<WeatherForecastEntry>> ParseExcelForecast(string file)
    {
        List<WeatherForecastEntry> result = [];
        FileInfo forecastFile = new(file);
        using var package = new ExcelPackage(forecastFile);
        await package.LoadAsync(forecastFile);
        


        foreach (var workSheet in package.Workbook.Worksheets)
        {
            try
            {
                ParseWorkSheet(workSheet, result);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error while parsing excel worksheet with message: {message}", exception.Message);
            }
        }
        if (forecastFile.Exists)
        {
            forecastFile.Delete();
        }
        return result;
    }

    private void ParseWorkSheet(ExcelWorksheet workSheet, List<WeatherForecastEntry> result)
    {
        int row = FindStartRow(workSheet);
        while (!string.IsNullOrWhiteSpace(workSheet.Cells[row, dateCol].Value?.ToString()))
        {
            try
            {
                WeatherForecastEntry model = new();
                model.Date = DateTime.Parse(workSheet.Cells[row, dateCol].Value.ToString());
                model.Temperature = double.Parse(workSheet.Cells[row, tempCol].Value.ToString());
                model.Humidity = double.Parse(workSheet.Cells[row, humidityCol].Value.ToString());
                model.AirPressure = double.Parse(workSheet.Cells[row, airPressureCol].Value.ToString());
                bool isWindy = double.TryParse(workSheet.Cells[row, windSpeedCol].Value.ToString(), out double windSpeed);
                model.WindSpeed = isWindy ? windSpeed : 0;
                bool isCloudy = double.TryParse(workSheet.Cells[row, cloudinessCol].Value.ToString(), out double cloudy);
                model.Cloudiness = isCloudy ? cloudy : 0;
                result.Add(model);
                row++;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
    /// <summary>
    /// Предполагаем, что дата находится в самом левом столбце
    /// </summary>
    /// <param name="workSheet"></param>
    /// <returns></returns>
    private int FindStartRow(ExcelWorksheet workSheet)
    {
        int row = 1;
        var cellValue = workSheet.Cells[row, dateCol].Value?.ToString();
        while (row < 10 && !string.IsNullOrEmpty(cellValue))
        { // Как способ проверки файла на валидность
            
            var isDate = DateTime.TryParse(cellValue, out DateTime _);
            if (isDate)
            {
                return row;
            }
            row++;
            cellValue = workSheet.Cells[row, dateCol].Value?.ToString();
        }
        return -1;
    }
}
