using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeSecureApi.Models;
using HomeSecureApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HomeSecureApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreamController : ControllerBase
    {

        private readonly StreamingManager _Mgr;

        private readonly HsConfig _Config;

        public StreamController(StreamingManager mgr, HsConfig config)
        {
            _Mgr=mgr;
            _Config=config;
        }


        [HttpGet]
        public Task<List<StreamInfo>> GetStreamInfo([FromQuery]string clientToken, CancellationToken cancel)
        {
            _Config.VerifyClientToken(clientToken);
            return _Mgr.GetStreamInfoAsync(cancel);
        }

        [HttpGet("{streamId}/Open")]
        public async Task<StreamSession> GetToken(
            [FromRoute]int streamId,
            [FromQuery]string clientToken,
            CancellationToken cancel)
        {
            _Config.VerifyClientToken(clientToken);
            return await _Mgr.OpenStreamAsync(streamId,cancel);
        }

        [HttpGet("{streamId}/Extend/{sessionId}")]
        public DateTime Extend(
            [FromRoute]int streamId,
            [FromRoute]Guid sessionId,
            [FromQuery]string clientToken,
            [FromQuery]string sessionToken,
            CancellationToken cancel)
        {
            _Config.VerifyClientToken(clientToken);
            return _Mgr.ExtendSession(streamId,sessionId,sessionToken);
        }

        [HttpGet("{streamId}/Close/{sessionId}")]
        public DateTime Close(
            [FromRoute]int streamId,
            [FromRoute]Guid sessionId,
            [FromQuery]string clientToken,
            [FromQuery]string sessionToken,
            CancellationToken cancel)
        {
            _Config.VerifyClientToken(clientToken);
            return _Mgr.CloseSession(streamId,sessionId,sessionToken);
        }
    }
}
