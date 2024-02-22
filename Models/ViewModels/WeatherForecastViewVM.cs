using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels;
public class WeatherForecastViewVM
{
    public enum GrouppingByType
    {
        None,
        ByMonth,
        ByYear
    }
    public GrouppingByType GrouppingType { get; set; } = GrouppingByType.None;
    public List<WeatherForecastEntry>? WeatherForecasts { get; set; }
}
