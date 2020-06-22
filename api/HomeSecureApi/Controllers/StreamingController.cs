using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSecureApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HomeSecureApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StreamingController : ControllerBase
    {

        private readonly StreamingManager _Mgr;

        public StreamingController(StreamingManager mgr)
        {
            _Mgr=mgr;
        }

        [HttpGet("Port")]
        public PortInfo GetToken([FromQuery]string clientToken)
        {
            return _Mgr.OpenPort(clientToken);
        }
    }
}
