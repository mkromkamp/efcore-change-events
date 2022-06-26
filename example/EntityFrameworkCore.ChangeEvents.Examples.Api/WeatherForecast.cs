using System.Text.Json.Serialization;
using HashidsNet;

namespace EntityFrameworkCore.ChangeEvents.Examples.Api;

public class WeatherForecast
{
    [JsonIgnore]
    public int? Id { get; set; }
    
    public string? EntryId => Id is null ? null : new Hashids(salt: "WeatherForecast", minHashLength: 6).Encode(Id.Value);
    
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }

    public void Update(WeatherForecast weatherForecast)
    {
        Date = weatherForecast.Date;
        TemperatureC = weatherForecast.TemperatureC;
        Summary = weatherForecast.Summary;
    }
}
