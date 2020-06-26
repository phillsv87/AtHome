using System;
using System.Collections.Generic;
using System.IO;
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
    public class MailNotifications: IDisposable
    {

        private readonly ILogger<MailNotifications> _Logger;

        private readonly StreamingManager _StreamingManager;

        private readonly NotificationsManager _NotificationsManager;

        private readonly HsConfig _Config;

        private readonly CancellationTokenSource _Cancel;
        
        public MailNotifications(
            ILogger<MailNotifications> logger,
            StreamingManager streamingManager,
            NotificationsManager notificationsManager,
            HsConfig config)
        {
            _Logger=logger;
            _StreamingManager=streamingManager;
            _NotificationsManager=notificationsManager;
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

        private readonly Regex BoundaryReg=new Regex(
            @"boundary=""([^""]+)""",
            RegexOptions.IgnoreCase|RegexOptions.Compiled);

        private readonly Regex EmailReg=new Regex(
            @"(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))",
            RegexOptions.IgnoreCase|RegexOptions.Compiled);


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

                            var data=_Config.SmtpDumpMessages?new StringBuilder():null;
                            var body=new StringBuilder();

                            bool inDataHeader=true;
                            bool inBoundaryHeader=false;
                            string to=null;
                            string from=null;
                            string boundary=null;
                            string partContentType=null;
                            string partEncoding=null;
                            string messageBody=null;
                            bool messageIsBase64=false;
                            List<byte[]> files=new List<byte[]>();


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
                                        data?.AppendLine(".");
                                    }else{
                                        data?.AppendLine(line);
                                    }

                                    if(line==".."){
                                        line=".";
                                    }

                                    if(inDataHeader){
                                        if(string.IsNullOrWhiteSpace(line)){
                                            inDataHeader=false;
                                            continue;
                                        }

                                        var ll=line.ToLower();
                                        if(ll.StartsWith("content-type:")){
                                            var match=BoundaryReg.Match(line);
                                            if(match.Success){
                                                boundary=match.Groups[1].Value;
                                            }
                                        }
                                        continue;
                                    }

                                    if(inBoundaryHeader){
                                        if(string.IsNullOrWhiteSpace(line)){
                                            inBoundaryHeader=false;
                                            continue;
                                        }
                                        var ll=line.ToLower();
                                        if(ll.StartsWith("content-type:")){
                                            partContentType=line.Trim().Trim(';');
                                        }else if(ll.StartsWith("content-transfer-encoding:")){
                                            partEncoding=line.Trim().Trim(';');
                                            messageIsBase64=partEncoding.ToLower()=="base64";
                                        }
                                        continue;
                                    }

                                    if(boundary==null){
                                        body.AppendLine(line);
                                    }else if(line.Contains(boundary)){
                                        if(partContentType!=null){
                                            switch(partContentType.ToLower()){
                                                case "text/plan":
                                                    messageBody=body.ToString();
                                                    if(messageIsBase64){
                                                        try{
                                                            var bytes=Convert.FromBase64String(messageBody);
                                                            messageBody=Encoding.UTF8.GetString(bytes);
                                                        }catch(Exception ex){
                                                            _Logger.LogError(ex,"Unable to convert base64 encoded message");
                                                        }
                                                    }
                                                    break;

                                                case "application/octet-stream":
                                                    if(messageIsBase64){
                                                        files.Add(Convert.FromBase64String(body.ToString()));
                                                    }else{
                                                        files.Add(Encoding.ASCII.GetBytes(body.ToString()));
                                                    }
                                                    break;
                                            }
                                        }
                                        partContentType=null;
                                        partEncoding=null;
                                        inBoundaryHeader=true;
                                        body.Clear();
                                    }else{
                                        if(messageIsBase64){
                                            body.Append(line);
                                        }else{
                                            body.AppendLine(line);
                                        }
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
                                        HandleMessageAsync(
                                            to,
                                            from,
                                            boundary==null?body.ToString():messageBody,
                                            files,
                                            _Cancel.Token).LogErrors();
                                        _continue=false;
                                        await writeLineAsync("221 Bye");
                                        client.Close();
                                        break;

                                    case "AUTH":
                                        await writeLineAsync("334 VXNlcm5hbWU6");// base64 for Username
                                        acceptUsername=true;
                                        break;

                                    case "RCPT":
                                        if(line.ToUpper().StartsWith("RCPT TO:")){
                                            var match=EmailReg.Match(line);
                                            if(match.Success){
                                                to=match.Value;
                                            }
                                        }
                                        await writeLineAsync($"250 Ok");
                                        break;

                                    case "MAIL":
                                        if(line.ToUpper().StartsWith("MAIL FROM:")){
                                            var match=EmailReg.Match(line);
                                            if(match.Success){
                                                from=match.Value;
                                            }
                                        }
                                        await writeLineAsync($"250 Ok");
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
                            _Logger.LogInformation("SMTP Client Closed Gracefully - "+address);
                            if(data!=null){
                                _Logger.LogInformation(data.ToString());
                            }
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

        private async Task HandleMessageAsync(
            string to, string from, string messageBody, List<byte[]> files, CancellationToken cancel)
        {

            if(from==null){
                _Logger.LogError("Null (from) supplied to HandleMessageAsync");
                return;
            }

            var tag=to.Split('@')[0];

            var stream=await _StreamingManager.GetStreamByTagAsync(tag,cancel);
            if(stream==null){
                _Logger.LogError($"No stream found by tag ({tag}) in HandleMessageAsync");
            }

            var alert=new LocationAlert(){
                Id=_Config.LocationId,
                Stream=stream==null?null:stream.GetInfo()  
            };

            await _NotificationsManager.SendNotificationAsync(
                    stream==null?tag:stream.Name,
                    new Dictionary<string, string>(){{
                        "appAction","LocationAlert:"+JsonConvert.SerializeObject(alert)}},
                cancel);

        }

    }
}