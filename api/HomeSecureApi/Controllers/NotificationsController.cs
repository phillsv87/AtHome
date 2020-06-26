using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HomeSecureApi.Models;
using HomeSecureApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HomeSecureApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController: ControllerBase
    {
        
        private readonly NotificationsManager _Mgr;

        private readonly HsConfig _Config;

        public NotificationsController(NotificationsManager mgr, HsConfig config)
        {
            _Mgr=mgr;
            _Config=config;
        }

        [HttpGet("Test")]
        public Task<List<NotificationsManager.PushNotificationResult>> SendTest([FromQuery]string key, [FromQuery]string message, CancellationToken cancel)
        {
            _Config.VerifyUtilAuth(key);
            return _Mgr.SendNotificationAsync(message,null,cancel);
        }

        [HttpPost("Device")]
        public Task<bool> AddDeviceAsync(
            NotificationDevice device,
            [FromQuery]string clientToken,
            CancellationToken cancel)
        {
            _Config.VerifyClientToken(clientToken);
            return _Mgr.AddDeviceAsync(device,cancel);
        }

        [HttpDelete("Device/{deviceId}")]
        public Task<bool> RemoveDeviceAsync(
            [FromRoute]string deviceId,
            [FromQuery]string clientToken,
            CancellationToken cancel)
        {
            _Config.VerifyClientToken(clientToken);
            return _Mgr.RemoveDeviceAsync(deviceId,cancel);
        }
    }
}