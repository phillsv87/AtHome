using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NblWebCommon;

namespace HomeSecureApi.Services
{
    [ServiceDescriptor(ServiceLifetime.Singleton)]
    public class MailNotifications: IDisposable
    {

        private readonly ILogger<MailNotifications> _Logger;

        private readonly CancellationTokenSource _Cancel;

        private readonly HsConfig _Config;
        
        public MailNotifications(ILogger<MailNotifications> logger, HsConfig config)
        {
            _Logger=logger;
            _Config=config;
            _Cancel=new CancellationTokenSource();
        }

        public void Dispose()
        {
            _Cancel.Cancel();
            _Cancel.SafeDispose();
        }

        public void Start()
        {
            _Logger.LogInformation("Starting MailNotifications service");
            RunSmtpAsync(_Cancel.Token).LogErrors();
        }

        private async Task RunSmtpAsync(CancellationToken cancel)
        {
            TcpListener listener=null;
            var cr=cancel.Register(()=>{
                if(listener!=null){
                    try{listener.Stop();}catch{}
                }
            });

            try{

                listener=new TcpListener(
                    new IPEndPoint(IPAddress.Parse(_Config.SmtpListenAddress),_Config.SmtpPort));

                listener.Start(20);

                _Logger.LogInformation($"SMTP listening on {_Config.SmtpListenAddress}:{_Config.SmtpPort}");

                while(!cancel.IsCancellationRequested){

                    try{
                    
                        using(var client=await listener.AcceptTcpClientAsync())
                        using(var stream=client.GetStream())
                        using(var reader=new StreamReader(stream))
                        using(var writer=new StreamWriter(stream))
                        {

                            writer.NewLine="\n";
                            writer.AutoFlush=true;

                            Func<string,Task> writeLineAsync=(line)=>{
                                if(_Config.SmtpDebug){
                                    Console.WriteLine("SMTP SEND: "+line);
                                }
                                return writer.WriteLineAsync(line);
                            };

                            var address=(client.Client.RemoteEndPoint as IPEndPoint)?.Address;

                            _Logger.LogInformation("SMTP Client Accepted - "+address);

                            await writeLineAsync($"220 localhost ESMTP Exim 4.66");

                            bool dataNext=false;
                            bool _continue=true;
                            bool acceptUsername=false;
                            bool acceptPassword=false;

                            var data=new StringBuilder();

                            while(_continue && !reader.EndOfStream){
                                
                                var line=await reader.ReadLineAsync();
                                if(_Config.SmtpDebug){
                                    Console.WriteLine("SMTP RECV: "+line);
                                }

                                if(dataNext){
                                    
                                    if(line=="."){
                                        dataNext=false;
                                        await writeLineAsync("250 Ok: queued");
                                        continue;
                                    }else if(line==".."){
                                        data.AppendLine(".");
                                    }else{
                                        data.AppendLine(line);
                                    }

                                    continue;
                                }

                                var i=line.IndexOf(' ');
                                var cmd=i==-1?line:line.Substring(0,i);

                                switch(cmd.ToUpper()){
                                    
                                    case "HELO":
                                        await writeLineAsync($"250 localhost Hello");
                                        break;
                                    
                                    case "EHLO":
                                        await writeLineAsync($"250-localhost Hello");
                                        await writeLineAsync($"250-SIZE 1000000");
                                        await writeLineAsync($"250 AUTH LOGIN");
                                        break;

                                    case "DATA":
                                        await writeLineAsync($"354 End data with <CR><LF>.<CR><LF>");
                                        dataNext=true;
                                        break;

                                    case "QUIT":
                                        _continue=false;
                                        await writeLineAsync("221 Bye");
                                        client.Close();
                                        break;

                                    case "AUTH":
                                        await writeLineAsync("334 VXNlcm5hbWU6");// base64 for Username
                                        acceptUsername=true;
                                        break;

                                    default:
                                        if(acceptUsername){
                                            await writeLineAsync("334 UGFzc3dvcmQ6");// base64 for Password
                                            acceptUsername=false;
                                            acceptPassword=true;
                                            break;
                                        }

                                        if(acceptPassword){
                                            await writeLineAsync("235 Authentication succeeded");
                                            acceptPassword=false;
                                            break;
                                        }
                                        await writeLineAsync($"250 Ok");
                                        break;
                                        
                                }

                            }
                            _Logger.LogInformation("SMTP Client Closed Gracefully - "+address+"\n"+data);
                        }
                        
                    }catch(Exception ex)
                    {
                        if(!cancel.IsCancellationRequested){
                            _Logger.LogError(ex,"RunSmtpAsync - client");
                        }
                    }

                }

            }catch(Exception ex)
            {
                if(!cancel.IsCancellationRequested){
                    _Logger.LogError(ex,"RunSmtpAsync");
                }
            }finally{
                if(listener!=null){
                    try{listener.Stop();}catch{}
                }
                cr.Dispose();
            }

            
        }

    }
}