using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HomeSecureApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NblWebCommon;
using Newtonsoft.Json;

namespace HomeSecureApi.Services
{
    [ServiceDescriptor(ServiceLifetime.Singleton)]
    public class StreamingManager: IDisposable
    {

        public const string StreamName="stream.m3u8";

        private readonly List<StreamSession> _Sessions=new List<StreamSession>();

        private readonly HsConfig _Config;

        private List<StreamConfig> _StreamConfigs;

        private readonly ILogger<StreamingManager> _Logger;

        private readonly object _Sync=new object();

        private readonly CancellationTokenSource _Cancel;

        private bool _FirstStream=true;

        public StreamingManager(HsConfig config, ILogger<StreamingManager> logger)
        {
            _Config=config;
            _Logger=logger;
            _Cancel=new CancellationTokenSource();
        }

        public void Dispose()
        {
            _Cancel.Cancel();
            _Cancel.SafeDispose();
        }

        private async Task<List<StreamConfig>> GetStreamConfigsAsync(CancellationToken cancel)
        {
            if(_StreamConfigs!=null){
                return _StreamConfigs;
            }

            var path=_Config.StreamsConfig;
            if(!File.Exists(path)){
                throw new InvalidConfigException("StreamsConfig file does not exists - "+path);
            }

            var json=await File.ReadAllTextAsync(path,cancel);
            _StreamConfigs=JsonConvert.DeserializeObject<List<StreamConfig>>(json);
            return _StreamConfigs;
        }

        private StreamSession GetSession(int streamId, Guid sessionId, string sessionToken)
        {
            StreamSession session;
            lock(_Sync){
                session=_Sessions.FirstOrDefault(s=>s.Id==sessionId);
            }

            if(session==null){
                throw new NotFoundException("No session found by Id");
            }

            if(session.Token!=sessionToken){
                throw new UnauthorizedException();
            }

            return session;
        }

        /// <summary>
        /// Get a list of all streams availble
        /// </summary>
        public async Task<List<StreamInfo>> GetStreamInfoAsync(string clientToken,CancellationToken cancel)
        {

            if(clientToken!=_Config.ClientToken){
                throw new UnauthorizedException();
            }

            var streams=await GetStreamConfigsAsync(cancel);

            return streams.Select(s=>s.GetInfo()).ToList();

        }

        /// <summary>
        /// Closes a StreamSession
        /// </summary>
        public DateTime CloseSession(int streamId, Guid sessionId, string sessionToken, string clientToken)
        {
            if(clientToken!=_Config.ClientToken){
                throw new UnauthorizedException();
            }

            var session=GetSession(streamId,sessionId,sessionToken);

            session.Expirers=DateTime.UtcNow.AddMinutes(-1);

            return session.Expirers;
        }

        /// <summary>
        /// Extends a StreamSession
        /// </summary>
        public DateTime ExtendSession(int streamId, Guid sessionId, string sessionToken, string clientToken)
        {
            if(clientToken!=_Config.ClientToken){
                throw new UnauthorizedException();
            }

            var session=GetSession(streamId,sessionId,sessionToken);

            session.Expirers=DateTime.UtcNow.AddSeconds(_Config.ClientSessionTTLSeconds);

            return session.Expirers;
        }


        /// <summary>
        /// Starts a new stream session. Call ExtendSession to extend the session.
        /// </summary>
        public async Task<StreamSession> OpenStreamAsync(
            int streamId,
            string clientToken,
            CancellationToken cancel)
        {

            if(clientToken!=_Config.ClientToken){
                throw new UnauthorizedException();
            }

            var streamConfig=(await GetStreamConfigsAsync(cancel)).FirstOrDefault(s=>s.Id==streamId);

            if(streamConfig==null){
                throw new NotFoundException("No stream found by Id");
            }

            var token=Rando.GetReallyRandomString(30);
            var dirToken=Rando.GetReallyRandomString(30);
            var session=new StreamSession()
            {
                Id=Guid.NewGuid(),
                StreamId=streamConfig.Id,
                StreamName=streamConfig.Name,
                Token=token,
                Expirers=DateTime.UtcNow.AddSeconds(_Config.ClientSessionTTLSeconds),
                TTLSeconds=_Config.ClientSessionTTLSeconds,
                Uri=$"Stream/{dirToken}/{StreamName}"
            };

            lock(_Sync){
                _Sessions.Add(session);
            }

            var ready=new TaskCompletionSource<bool>();
            RunStreamAsync(session,streamConfig,dirToken,ready,cancel,_Cancel.Token).LogErrors();

            using(cancel.Register(()=>{
                ready.TrySetCanceled();
            }))
            {
                await ready.Task;
            }

            return session;
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task RunStreamAsync(
            StreamSession session,
            StreamConfig streamConfig,
            string dirToken,
            TaskCompletionSource<bool> ready,
            CancellationToken clientCancel,
            CancellationToken cancel)
        {

            Process proc=null;
            var root=_Config.HlsRoot;
            if(string.IsNullOrWhiteSpace(root)){
                throw new InvalidConfigException("HlsRoot not set");
            }

            var cmd=_Config.HlsCommand;
            if(string.IsNullOrWhiteSpace(cmd)){
                throw new InvalidConfigException("HlsCommand not set");
            }

            var streamDir=Path.Combine(root,dirToken);
            if(Directory.Exists(streamDir)){
                throw new InvalidOperationException("streamDir already exists");
            }
            
            try{

                lock(_Sync){
                    if(_FirstStream){
                        _FirstStream=false;

                        var files=Directory.GetFiles(root);
                        var dirs=Directory.GetDirectories(root);
                        foreach(var file in files){
                            try{
                                File.Delete(file);
                            }catch(Exception ex){
                                _Logger.LogWarning(ex,"Unable to delete old hls file "+file);
                            }
                        }
                        foreach(var dir in dirs){
                            try{
                                Directory.Delete(dir,true);
                            }catch(Exception ex){
                                _Logger.LogWarning(ex,"Unable to delete old hls directory "+dir);
                            }
                        }

                        _Logger.LogInformation("HLS cleanup complete");
                    }
                }
                
                Directory.CreateDirectory(streamDir);

                
                cmd=cmd
                    .Replace("{INPUT}",streamConfig.Uri)
                    .Replace("{STREAM_NAME}",StreamName);
                var args=cmd.Split(' ',2);

                proc=new Process(){
                    StartInfo=new ProcessStartInfo()
                    {
                        WorkingDirectory=streamDir,
                        FileName=args[0],
                        Arguments=args.Length==1?null:args[1]
                    }
                };

                proc.Start();

                var timeout=DateTime.UtcNow.AddSeconds(_Config.StreamStartTimeoutSeconds);
                while(true)
                {
                    if(DateTime.UtcNow>timeout){
                        throw new RequestException(408,"Start start timeout");
                    }
                    await Task.Delay(20);
                    var files=Directory.GetFiles(streamDir,"*.m3u8");
                    var exists=files.Any(f=>f.EndsWith(StreamName));
                    cancel.ThrowIfCancellationRequested();
                    clientCancel.ThrowIfCancellationRequested();
                    if(exists){
                        break;
                    }
                }

                ready.TrySetResult(true);

                var n=DateTime.UtcNow;

                while(!cancel.IsCancellationRequested){

                    await Task.Delay(5000);

                    if(session.Expirers<DateTime.UtcNow){
                        _Logger.LogInformation("Client Stream expired");
                        break;
                    }

                    proc.Refresh();
                    if(proc.HasExited){
                        throw new Exception("stream commanded existed with exit code "+proc.ExitCode);
                    }

                }

            }catch(Exception ex){
                if(!cancel.IsCancellationRequested && !(!ready.Task.IsCompleted && cancel.IsCancellationRequested)){
                    _Logger.LogError(ex,"RunStreamAsync");
                }
            }finally{
                if(proc!=null){
                    try{proc.Kill();}catch{}
                    proc.SafeDispose();
                }
                try{
                    if(Directory.Exists(streamDir)){
                        Directory.Delete(streamDir,true);
                    }
                }catch{}
                lock(_Sync){
                    _Sessions.Remove(session);
                }
            }

        }
    }
}