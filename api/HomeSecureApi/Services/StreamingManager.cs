using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NblWebCommon;

namespace HomeSecureApi.Services
{
    [ServiceDescriptor(ServiceLifetime.Singleton)]
    public class StreamingManager: IDisposable
    {

        private class PrivatePortInfo
        {
            public PortInfo Info{get;}

            public bool Connected{get;set;}

            public PrivatePortInfo(PortInfo info)
            {
                Info=info;
            }
        }

        private readonly List<PrivatePortInfo> _OpenPorts=new List<PrivatePortInfo>();

        private readonly HsConfig _Config;

        private readonly ILogger<StreamingManager> _Logger;

        private readonly object _Sync=new object();

        private readonly CancellationTokenSource _Cancel;

        public StreamingManager(HsConfig config, ILogger<StreamingManager> logger)
        {
            _Config=config;
            _Logger=logger;
            _Cancel=new CancellationTokenSource();
            RunListenerAsync(_Cancel.Token).LogErrors();
        }

        public void Dispose()
        {
            _Cancel.Cancel();
            _Cancel.SafeDispose();
        }


        public PortInfo OpenPort(string clientToken)
        {

            if(clientToken!=_Config.ClientToken){
                throw new UnauthorizedException();
            }

            PortInfo info=new PortInfo()
            {
                Port=_Config.StreamingPort,
                Token=Rando.GetReallyRandomString(30)
            };

            lock(_Sync){
                _OpenPorts.Add(new PrivatePortInfo(info));
            }

            return info;
        }

        private const int BufSize=2_000_000;

        private async Task RunListenerAsync(CancellationToken cancel)
        {
            var listener=new TcpListener(new IPEndPoint(0,_Config.StreamingPort));
            listener.Start();

            using(cancel.Register(()=>{
                try{
                    listener.Stop();
                }catch{}
            }))
            {
                await Task.Delay(10).ConfigureAwait(false);

                while(!cancel.IsCancellationRequested){

                    var client=await listener.AcceptTcpClientAsync();
                    RunClientAsync(client,cancel).LogErrors();

                }
            }
        }

        private readonly Regex HeaderReg=new Regex(
            @"/\Wtoken=(\w+)",
            RegexOptions.IgnoreCase|RegexOptions.Compiled);

        private async Task RunClientAsync(TcpClient client, CancellationToken cancel)
        {

            client.NoDelay=true;
            client.SendBufferSize=BufSize+100;
            var stream=client.GetStream();

            try{

                var buf=new byte[BufSize];
                int l=0;

                // var t=await stream.ReadAsync(buf,0,buf.Length,cancel);
                // Console.WriteLine(Encoding.UTF8.GetString(buf,0,t));
                // return;

                while(l<300){
                    var x=await stream.ReadAsync(buf,l,1,cancel);
                    if(x==0){//end of stream
                        throw new EndOfStreamException();
                    }
                    l++;

                    if(buf[l-1]=='\n' || buf[l-1]=='\r'){ // end of header
                        break;
                    }
                }

                var header=Encoding.UTF8.GetString(buf,0,l-1);
                var match=HeaderReg.Match(header);
                if(!match.Success){
                    throw new BadRequestException("Bad stream Uri");
                }

                var token=match.Groups[2].Value;

                PrivatePortInfo info;

                lock(_Sync){
                    info=_OpenPorts.FirstOrDefault(p=>p.Info.Token==token);
                    if(info==null || info.Connected){
                        throw new UnauthorizedException();
                    }
                    info.Connected=true;
                }

                using(var rtspClient=new TcpClient())
                {
                    rtspClient.NoDelay=true;
                    rtspClient.ReceiveBufferSize=BufSize+100;
                    await rtspClient.ConnectAsync(IPAddress.Parse("192.168.55.101"),554);

                    using(var rtspStream=rtspClient.GetStream()){

                        await rtspStream.WriteAsync(buf,0,l);

                        var task1=CopyAsync(buf,stream,rtspStream,"Camera",cancel);
                        var task2=CopyAsync(new byte[4096],rtspStream,stream,"FFPlay",cancel);
                        
                        await task1;
                        await task2;
                    }
                }


            }catch(Exception ex){

                _Logger.LogInformation(ex,"RunClientAsync");

            }finally{
                stream.SafeDispose();
                try{client.Close();}catch{}
                client.SafeDispose();
            }
            
        }

        private async Task CopyAsync(
            byte[] buf,
            Stream dest,
            Stream src,
            string printLabel,
            CancellationToken cancel)
        {
            while(!cancel.IsCancellationRequested){
                var l=await src.ReadAsync(buf,0,buf.Length,cancel);
                if(l==0){
                    break;
                }
                if(printLabel!=null){
                    Console.WriteLine(printLabel+" - "+Encoding.ASCII.GetString(buf,0,l));
                }
                await dest.WriteAsync(buf,0,l,cancel);
            }
        }
    }
}