using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSecureApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NblWebCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HomeSecureApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private HsConfig _Config;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.LoadPrefixValues(
                "HS_",
                "../../apiconfig.json;../../secrets/apiconfig-secrets.json");

            _Config=services.AddPrefixConfig<HsConfig>();

            services
                .AddUsingDescriptor<StreamingManager>()
                .AddUsingDescriptor<MailNotifications>()
                .AddUsingDescriptor<NotificationsManager>();
                
            services
                .AddControllers()
                .AddNewtonsoftJson(o=>{
                    o.SerializerSettings.ContractResolver=new DefaultContractResolver();
                    o.SerializerSettings.ReferenceLoopHandling=ReferenceLoopHandling.Ignore;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            MailNotifications mailNotifications)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var provider = new FileExtensionContentTypeProvider();
                provider.Mappings[".ts"] = "video/mp2t";
                provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    System.IO.Path.GetFullPath(_Config.HlsRoot)
                ),
                RequestPath = "/Stream",
                ContentTypeProvider=provider
            });

            app.UseMiddleware<HttpExceptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            mailNotifications.Start();
        }
    }
}
