using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using POC_Saml.Helpers;
using POC_Saml.Models.Request;

namespace POC_Saml.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly Saml2Configuration _samlConfig;

    public WeatherForecastController(ILogger<WeatherForecastController> logger,
        IOptions<Saml2Configuration> configAccessor)
    {
        _logger = logger;
        _samlConfig = configAccessor.Value;
    }

    
    //Necessary add this rout on Azure URIS redirecting 
    [HttpPost(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Post([FromForm] AzureAdRequestCallbackRequest request)
    {
        var xmlSamlResponseDecrypted = Base64Helper.DecodeBase64(request.SAMLResponse);
        
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [Route("Login")]
    public IActionResult Login(string? returnUrl )
    {
        try
        {
            _logger.LogInformation("Start endpoint of sso controller invoked.");

            var binding = new Saml2RedirectBinding();
            
            binding.SetRelayStateQuery(new Dictionary<string, string>()
            {
                { "relayStateReturnUrl", $"https://mail.google.com" }
            });

            return binding.Bind(new Saml2AuthnRequest(_samlConfig)).ToActionResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return BadRequest("Error");
        }
    }
}