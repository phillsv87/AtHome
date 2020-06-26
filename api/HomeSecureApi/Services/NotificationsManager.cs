using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HomeSecureApi.Models;
using Jose;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NblWebCommon;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;

namespace HomeSecureApi.Services
{
    [ServiceDescriptor(ServiceLifetime.Singleton)]
    public class NotificationsManager: IDisposable
    {

        private readonly ILogger _Logger;
        private readonly HsConfig _Config;
        private readonly HttpClient _Client;

        private readonly SemaphoreSlim _Sync=new SemaphoreSlim(1);

        private List<NotificationDevice> _Devices=null;

        public NotificationsManager(
            ILogger<NotificationsManager> logger,
            HsConfig config)
        {
            _Logger = logger;
            _Config = config;
            _Client = new HttpClient();
        }

        public void Dispose()
        {
            _Sync.SafeDispose();
        }

        
        private async Task<List<NotificationDevice>> GetDevicesAsync(CancellationToken cancel)
        {
            if(_Devices!=null){
                return _Devices;
            }

            var devices=File.Exists(_Config.NotificationDeviceDb)?
                JsonConvert.DeserializeObject<List<NotificationDevice>>(
                    await File.ReadAllTextAsync(_Config.NotificationDeviceDb,cancel)):
                new List<NotificationDevice>();

            await _Sync.WaitAsync(cancel);
            try{
                if(_Devices==null){
                    _Devices=devices;
                }
            }finally{
                _Sync.Release();
            }
            
            return _Devices;
        }

        private async Task SaveDevicesAsync()
        {
            if(_Devices==null){
                return;
            }

            var json=JsonConvert.SerializeObject(_Devices);

            await File.WriteAllTextAsync(_Config.NotificationDeviceDb,json);
        }

        public async Task<bool> AddDeviceAsync(NotificationDevice device, CancellationToken cancel)
        {
            if(device==null){
                return false;
            }

            var devices=await GetDevicesAsync(cancel);

            await _Sync.WaitAsync(cancel);
            try{
                var match=devices.FirstOrDefault(d=>d.Id==device.Id);
                if(match==null){
                    devices.Add(device);
                }else if(match.AreSame(device)){
                    return false;
                }else{
                    devices[devices.IndexOf(match)]=device;
                }

                await SaveDevicesAsync();

                return true;
                

            }finally{
                _Sync.Release();
            }
        }

        public async Task<bool> RemoveDeviceAsync(string deviceId, CancellationToken cancel)
        {
            if(deviceId==null){
                return false;
            }

            var devices=await GetDevicesAsync(cancel);

            await _Sync.WaitAsync(cancel);
            try{
                var match=devices.FirstOrDefault(d=>d.Id==deviceId);
                if(match==null){
                    return false;
                }

                devices.Remove(match);

                await SaveDevicesAsync();

                return true;
                

            }finally{
                _Sync.Release();
            }
        }


        public async Task<List<PushNotificationResult>> SendNotificationAsync(
            string message,
            [AllowNull]Dictionary<string,string> customKeys,
            CancellationToken cancel)
        {
            var devices=await GetDevicesAsync(cancel);

            var results=new List<PushNotificationResult>();


            foreach(var d in devices){
                cancel.ThrowIfCancellationRequested();
                var r=await SendNotificationAsync(d.Id,d.Type,message,customKeys,cancel);
                results.Add(r);
            }

            return results;

        }

        public async Task<PushNotificationResult> SendNotificationAsync(
            string deviceId,
            NotificationType type,
            string message,
            [AllowNull]Dictionary<string,string> customKeys,
            CancellationToken cancel)
        {
            PushNotificationResultType rt;
            switch(type){
                case NotificationType.APN:
                    rt=await SendIosPushNotificationAsync(deviceId,message,customKeys,cancel);
                    break;
                default:
                    rt=PushNotificationResultType.NotSent;
                    break;
            }
            return new PushNotificationResult(){
                Type=rt,
                DeviceId=deviceId
            };
        }

        public async Task<PushNotificationResultType> SendIosPushNotificationAsync(
            string deviceId,
            string message,
            Dictionary<string,string> customKeys,
            CancellationToken cancel)
        {

            var token = await GetTokenAsync(cancel);

            var json=
                "{\"aps\":{\"alert\":" +
                JsonConvert.SerializeObject(message)+
                ",\"badge\":0,\"sound\":\"default\"}";

            if(customKeys!=null){
                foreach(var p in customKeys){
                    json+=","+JsonConvert.SerializeObject(p.Key)+":"+JsonConvert.SerializeObject(p.Value);
                }
            }

            var content = new StringContent(
                json+"}",
                Encoding.UTF8,
                "application/json");

            var msg = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://{_Config.ApnDomain}/3/device/{Uri.EscapeDataString(deviceId)}");
            msg.Headers.Authorization = token.Value;
            msg.Headers.Add("apns-id", Guid.NewGuid().ToString().ToLower());
            msg.Headers.Add("apns-priority", "10");
            msg.Headers.Add("apns-topic", _Config.ApnTopic);
            msg.Version = new Version(2, 0);
            msg.Content = content;

            try
            {

                var result = await _Client.SendAsync(msg, cancel);
                if (!result.IsSuccessStatusCode)
                {
                    var r = await result.Content.ReadAsStringAsync();
                    _Logger.LogError(
                        $"Send push to APN endpoint failed. " +
                        $"StatusCode:{result.StatusCode}. Message:{r}");

                    bool isBadDeviceToken=false;
                    try{
                        var iosError=JsonConvert.DeserializeObject<ApnErrorResult>(r);
                        isBadDeviceToken=iosError.reason=="BadDeviceToken";
                    }catch(Exception ex){
                        _Logger.LogError(ex,$"Unable to deserialize APN error result");
                    }
                    return isBadDeviceToken?PushNotificationResultType.BadDevice:PushNotificationResultType.NotSent;
                }
            }
            catch(Exception ex)
            {
                _Logger.LogError(
                    $"Send push to APN endpoint failed - {_Config.ApnDomain}", ex);
            }

            return PushNotificationResultType.Sent;
        }

        private struct ApnErrorResult
        {
            public string reason{get;set;}
        }

        public enum PushNotificationResultType
        {
            Sent = 0,
            NotSent = 1,
            BadDevice = 2
        }

        public struct PushNotificationResult
        {
            public PushNotificationResultType Type{get;set;}
            public string DeviceId{get;set;}
        } 

        private class Token
        {
            public DateTime Expires;
            public AuthenticationHeaderValue Value;
        }

        private Token _Token;

        private async Task<Token> GetTokenAsync(CancellationToken cancel)
        {
            var t = _Token;
            if (t != null && t.Expires > DateTime.UtcNow)
            {
                return t;
            }

            t = new Token();
            t.Expires = DateTime.UtcNow.AddMinutes(55);

            var payload = new Dictionary<string, object>()
            {
                { "iat", (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds },
                { "iss", _Config.ApnIss },
            };

            var extraHeader = new Dictionary<string, object>()
            {
                { "alg", "ES256" },
                { "typ", "JWT" },
                { "kid", _Config.ApnKid }
            };

            var privateKey = await GetECDsaAsync(cancel);

            t.Value = new AuthenticationHeaderValue(
                "bearer",
                Jose.JWT.Encode(payload, privateKey, JwsAlgorithm.ES256, extraHeader));

            _Token = t;

            return t;
        }

        private async Task<ECDsa> GetECDsaAsync(CancellationToken cancel)
        {

            string key;
            if (_Config.ApnSecret != null)
            {
                key = _Config.ApnSecret;
            }
            else
            {
                key = await File.ReadAllTextAsync(_Config.ApnSecretPath, cancel);
            }

            using (TextReader reader = new StringReader(key))
            {
                var ecPrivateKeyParameters = (ECPrivateKeyParameters)
                    new Org.BouncyCastle.OpenSsl.PemReader(reader).ReadObject();

                Org.BouncyCastle.Math.EC.ECPoint q =
                    ecPrivateKeyParameters.Parameters.G
                    .Multiply(ecPrivateKeyParameters.D).Normalize();

                var d = ecPrivateKeyParameters.D.ToByteArrayUnsigned();


                var msEcp = new ECParameters();
                msEcp.Curve = ECCurve.NamedCurves.nistP256;
                msEcp.Q.X = q.AffineXCoord.GetEncoded();
                msEcp.Q.Y = q.AffineYCoord.GetEncoded();
                msEcp.D = d;
                return ECDsa.Create(msEcp);
            }
        }
        

    }
}
