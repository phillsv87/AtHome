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

        public StreamController(StreamingManager mgr)
        {
            _Mgr=mgr;
        }


        [HttpGet]
        public Task<List<StreamInfo>> GetStreamInfo([FromQuery]string clientToken, CancellationToken cancel)
        {
            return _Mgr.GetStreamInfoAsync(clientToken,cancel);
        }

        [HttpGet("{streamId}/Open")]
        public async Task<StreamSession> GetToken(
            [FromRoute]int streamId,
            [FromQuery]string clientToken,
            CancellationToken cancel)
        {
            return await _Mgr.OpenStreamAsync(streamId,clientToken,cancel);
        }

        [HttpGet("{streamId}/Extend/{sessionId}")]
        public DateTime Extend(
            [FromRoute]int streamId,
            [FromRoute]Guid sessionId,
            [FromQuery]string clientToken,
            [FromQuery]string sessionToken,
            CancellationToken cancel)
        {
            return _Mgr.ExtendSession(streamId,sessionId,sessionToken,clientToken);
        }

        [HttpGet("{streamId}/Close/{sessionId}")]
        public DateTime Close(
            [FromRoute]int streamId,
            [FromRoute]Guid sessionId,
            [FromQuery]string clientToken,
            [FromQuery]string sessionToken,
            CancellationToken cancel)
        {
            return _Mgr.CloseSession(streamId,sessionId,sessionToken,clientToken);
        }
    }
}
