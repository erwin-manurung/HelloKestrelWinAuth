using HelloKestrelWinAuth.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Data;
using System.Globalization;
using System.Reflection.Metadata;

namespace HelloKestrel.Controllers
{
    [Authorize(AuthenticationSchemes = "Negotiate")]
    //[AllowAnonymous]
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> GetSimpleWeather()
        {
            return Get();
        }
        [HttpGet(Name = "GetAnotherWeatherForecast")]
        public IEnumerable<WeatherForecast> GetAnother()
        {
            return Get();
        }

        [HttpGet]
        public IActionResult UpdateAnother([FromQuery] ComplexDto parameters)
        {
            return new OkResult();
        }

        [HttpPost]
        public ComplexDto UpdateComplex(ComplexDto parameters)
        {
            return new ComplexDto();
        }

        /// <summary>
        /// DataSet perlu ini
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateAnotherComplex(UpdateAnotherComplexParam param)
        {
            var s = param.dataSet;
            return new OkResult();
        }

    }

    public class UpdateAnotherComplexParam
    {
        public DataSet dataSet { get; set; }
        public ComplexDto complexDto { get; set; }
    }
}