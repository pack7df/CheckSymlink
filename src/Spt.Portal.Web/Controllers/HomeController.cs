using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Spt.Portal.Web.Models;

namespace Spt.Portal.Web.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        private AppConfig _appConfig;
        private readonly IConfiguration _configuration;

        protected AppConfig AppConfig
        {
            get
            {
                if (_appConfig == null)
                {
                    StringValues forwardHost;
                    if (Request.Headers.TryGetValue("X-Forwarded-Host", out forwardHost))
                    {
                        forwardHost = $"{Request.Scheme}://{forwardHost}";
                    }
                    else
                    {
                        forwardHost = $"{Request.Scheme}://{Request.Host}";
                    }

                    _appConfig = new AppConfig
                    {
                        ClientId = $"portal",
                        Scopes = $"openid profile email roles",
                        ServiceUrl = $"{forwardHost}",
                        IdentityServerUrl = _configuration["NativeClientOAuthConfiguration:Authority"]
                    };
                }
                return _appConfig;
            }
        }

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("api/config")]
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetConfig()
        {
            return Ok(AppConfig);
        }
    }
}
