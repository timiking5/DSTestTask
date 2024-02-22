using System.ComponentModel.DataAnnotations;

namespace Models;

/// <summary>
/// Т.к. в отображаемых данных мы видим отчёт за месяц/год, я решил оставить только те Properties,
/// которые можно агреггировать по среднему, например. Конечно, в базе данных можно хранить все значения
/// из Excel-документа, но мне захотелось сделать так.
/// </summary>
public class WeatherForecastEntry
{
    [Key]
    public long Id { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public double Temperature { get; set; }
    [Required]
    public double Humidity { get; set; }  // влажность
    /// <summary>
    /// мм рт. столба
    /// </summary>
    [Required]
    public double AirPressure { get; set; }
    [Required]
    public double WindSpeed { get; set; }
    [Required]
    public double Cloudiness { get; set; }
}
