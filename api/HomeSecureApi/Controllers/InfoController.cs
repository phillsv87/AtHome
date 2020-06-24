using System;
using HomeSecureApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HomeSecureApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfoController: ControllerBase
    {
        private static string _Version;

        private readonly HsConfig _Config;

        public InfoController(HsConfig config)
        {
            _Config=config;
        }

        [HttpGet]
        public ApiInfo Get()
        {
            if (_Version == null)
            {
                var v = typeof(InfoController).Assembly.GetName().Version;
                _Version = $"{v.Major}.{v.Minor}.{v.Build}";
            }

            return new ApiInfo(){
                Version=_Version,
                Id=string.IsNullOrWhiteSpace(_Config.LocationId)?Guid.Empty:Guid.Parse(_Config.LocationId)
            };
        }


    }
}