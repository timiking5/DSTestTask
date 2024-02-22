using DataAccess;
using DSTestTask.Services.WeatherExcelParser;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Models.ViewModels.WeatherForecastViewVM;
using Microsoft.Extensions.Caching.Distributed;
using DSTestTask.Extensions;

namespace DSTestTask.Controllers;
public class WeatherForecastController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IDistributedCache _cache;
    private readonly IWeatherParser _weatherParser;
    private readonly ApplicationDbContext _db;
    private readonly List<SelectListItem> grouppingOptions =
    [
        new () {Text = "By Month", Value = "1"},
        new () {Text = "By Year", Value = "2"}
    ];

    public WeatherForecastController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext db, IWeatherParser weatherParser, IDistributedCache cache)
    {
        _webHostEnvironment = webHostEnvironment;
        _db = db;
        _weatherParser = weatherParser;
        _cache = cache;
    }

    public IActionResult Upload()
    {
        WeatherForecastUploadVM vm = new();
        return View(vm);
    }
    [HttpPost]
    public async Task<IActionResult> Upload(WeatherForecastUploadVM vm)
    {
        if (vm.Files is null)
        {
            return View();
        }
        foreach (var file in vm.Files)
        {
            var extension = Path.GetExtension(file.FileName);
            if (extension != ".xlsx")
            {
                continue;
            }
            string fileName = Guid.NewGuid().ToString() + extension;
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string filePath = Path.Combine(wwwRootPath, $"weatherForecasts\\excelFiles\\{fileName}");
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            var forecastList = await _weatherParser.ParseExcelForecast(filePath);
            _db.WeatherForecastEntries.AddRange(forecastList);
            _db.SaveChanges();
        }
        return View();
    }
    public IActionResult ViewForecasts()
    {
        ViewData["GrouppingTypes"] = grouppingOptions;
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> ViewForecasts(WeatherForecastViewVM vm)
    {
        ViewData["GrouppingTypes"] = grouppingOptions;
        await LoadWeatherForecasts(vm);
        return View(vm);
    }

    private async Task LoadWeatherForecasts(WeatherForecastViewVM vm)
    {
        List<WeatherForecastEntry>? list = null;
        if (vm.GrouppingType == GrouppingByType.ByMonth)
        {
            string recordKey = "WeatherForecast_byMonth_" + DateTime.Now.ToString("yyyyMMdd_hhmm");
            list = await _cache.GetRecordAsync<List<WeatherForecastEntry>>(recordKey);
            if (list is null)
            {
                list = _db.WeatherForecastEntries.GroupBy(x => new { x.Date.Year, x.Date.Month })
                .Select(gcs => new WeatherForecastEntry
                {
                    Date = new DateTime(gcs.Key.Year, gcs.Key.Month, 1),  // standing for first day of month
                    Temperature = gcs.Average(x => x.Temperature),
                    WindSpeed = gcs.Average(x => x.WindSpeed),
                    AirPressure = gcs.Average(x => x.AirPressure),
                    Cloudiness = gcs.Average(x => x.Cloudiness),
                    Humidity = gcs.Average(x => x.Cloudiness)
                })
                .ToList();
                await _cache.SetRecordAsync<List<WeatherForecastEntry>>(recordKey, list);
            }
        }
        else if (vm.GrouppingType == GrouppingByType.ByYear)
        {
            string recordKey = "WeatherForecast_byYear_" + DateTime.Now.ToString("yyyyMMdd_hhmm");
            list = await _cache.GetRecordAsync<List<WeatherForecastEntry>>(recordKey);
            if (list is null)
            {
                list = _db.WeatherForecastEntries.GroupBy(x => new { x.Date.Year })
                .Select(gcs => new WeatherForecastEntry
                {
                    Date = new DateTime(gcs.Key.Year, 1, 1),  // standing for first day of month
                    Temperature = gcs.Average(x => x.Temperature),
                    WindSpeed = gcs.Average(x => x.WindSpeed),
                    AirPressure = gcs.Average(x => x.AirPressure),
                    Cloudiness = gcs.Average(x => x.Cloudiness),
                    Humidity = gcs.Average(x => x.Cloudiness)
                })
                .ToList();
                await _cache.SetRecordAsync(recordKey, list);
            }
        }
        vm.WeatherForecasts = list;
        vm.WeatherForecasts.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
    }
}
