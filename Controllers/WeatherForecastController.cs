using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IAntiforgery _antiforgery;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IAntiforgery antiforgery)
        {
            _logger = logger;
            _antiforgery = antiforgery;
        }
        [HttpPost]
        [Route("Get")]
        //[EnableCors("AllowSpecificOrigin")]
        [ValidateAntiForgeryToken]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            try
            {
                var asd = HttpContext.User.Identity;
                await _antiforgery.ValidateRequestAsync(HttpContext);
            }
            catch (Exception ex) { }
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("GetAntiforgeryToken")]
        public async Task<AntiforgeryTokenSet> GetAntiforgeryToken()
        {
            var tokens = new AntiforgeryTokenSet(string.Empty, string.Empty, string.Empty, string.Empty);
            try
            {
                tokens = _antiforgery.GetAndStoreTokens(HttpContext);

                Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions vara = new Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions();
                var dd = vara.AutoGenerateKeys;
            }
            catch (Exception ex) { }

            return tokens;
        }

        [HttpGet]
        [Route("UnAuthorized")]
        public async Task<IActionResult> UnAuthorized()
        {
            return BadRequest("Antiforgery error");
        }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class ValidateAntiForgeryTokenAttribute : Attribute, IFilterFactory, IOrderedFilter
        {
            public int Order { get; set; } = 1000;

            public bool IsReusable => true;

            public IHttpContextAccessor httpContext { get => _httpContextAccessor; }

            private IHttpContextAccessor _httpContextAccessor;

            public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                var antiforgery = serviceProvider.GetRequiredService<IAntiforgery>();

                var instance = new ActualCsrfValidationFilter(httpContextAccessor, antiforgery);

                return instance;
            }
            private class ActualCsrfValidationFilter : IAuthorizationFilter
            {
                private readonly IHttpContextAccessor _httpContextAccessor;
                private readonly IAntiforgery _antiforgery;
                private readonly ILogger logger;

                public ActualCsrfValidationFilter(IHttpContextAccessor httpContextAccessor, IAntiforgery antiforgery)
                {
                    _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
                    _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));

                }

                public async void OnAuthorization(AuthorizationFilterContext context)
                {
                    try
                    {
                        if(!await ValidateRequestAsync().ConfigureAwait(true))
                        {
                            throw new AntiforgeryValidationException("AntiforgeryValidationException");
                        }
                    }
                    catch (Exception ex)
                    {
                        _httpContextAccessor.HttpContext.Request.Path = "/weatherforecast/UnAuthorized";
                        context.Result = new BadRequestResult();
                    }
                }

                private async Task<bool> ValidateRequestAsync() 
                {
                   return await _antiforgery.IsRequestValidAsync(_httpContextAccessor.HttpContext);
                }
            }
        }
    }
}
