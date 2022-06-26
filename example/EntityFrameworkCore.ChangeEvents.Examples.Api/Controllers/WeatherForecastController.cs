using HashidsNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.ChangeEvents.Examples.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly SampleContext _sampleContext;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, SampleContext sampleContext)
    {
        _logger = logger;
        _sampleContext = sampleContext;
    }

    [HttpGet]
    public IAsyncEnumerable<WeatherForecast> GetPaged(CancellationToken cancellationToken, int top = 10, int skip = 0)
    {
        return _sampleContext.WeatherForecasts.OrderByDescending(w => w.Date).Skip(skip).Take(top)
            .AsAsyncEnumerable();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(WeatherForecast weatherForecast, CancellationToken cancellationToken)
    {
        await _sampleContext.WeatherForecasts.AddAsync(weatherForecast, cancellationToken);
        await _sampleContext.SaveChangesAsync(cancellationToken);

        return Accepted();
    }

    [HttpGet("{entryId}")]
    public async Task<ActionResult<WeatherForecast>> GetById(string entryId, CancellationToken cancellationToken)
    {
        var id = new Hashids("WeatherForecast", 6).Decode(entryId).FirstOrDefault();
        var forecast = await _sampleContext.WeatherForecasts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return forecast is null ? NotFound() : Ok(forecast);
    }
    
    [HttpPut("{entryId}")]
    public async Task<ActionResult<WeatherForecast>> Update(string entryId, WeatherForecast weatherForecast, CancellationToken cancellationToken)
    {
        var id = new Hashids("WeatherForecast", 6).Decode(entryId).FirstOrDefault();
        var forecast = await _sampleContext.WeatherForecasts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (forecast is null)
            return NotFound();

        forecast.Update(weatherForecast);
        await _sampleContext.SaveChangesAsync(cancellationToken);

        return Ok(forecast);
    }
    
    [HttpDelete("{entryId}")]
    public async Task<ActionResult<WeatherForecast>> Delete(string entryId, CancellationToken cancellationToken)
    {
        var id = new Hashids("WeatherForecast", 6).Decode(entryId).FirstOrDefault();
        var forecast = await _sampleContext.WeatherForecasts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (forecast is not null)
        {
            _sampleContext.WeatherForecasts.Remove(forecast);
            await _sampleContext.SaveChangesAsync(cancellationToken);
        }

        return forecast is null ? NotFound() : Accepted();
    }
}
