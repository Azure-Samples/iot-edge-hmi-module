/*
This code is called from Program.cs and activates other services and sets 
configurations that the WebApp Module uses.  This includes:
-Whether the WebApp using the InProcShelfDataGenerator.cs to generate data for 
UI testing or connects to ModuleClient.cs for live data.
-Adding HostedService: ClientNotifier.cs
-Adding HostedService: ClientUpdateHub.cs and set configuration 
  for the SignalR service
*/

using System;
using System.Collections.Concurrent;  //Added
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; 
using Microsoft.Azure.Devices.Client; 

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            /*InProcShelfDataGenerator is used for HMI testing so live data is not needed.  This 
              decouples the data generation debugging from debugging/modifying the HMI module.
              See the "UI Data Generation" section of the README for instructions on how to set 
              the Environment variable.

              If this environment variable is false - connect to the IoT Edge Hub module for data.
            */            
            var useInProcDataGeneratorEnvValue = Environment.GetEnvironmentVariable("UseInProcDataGenerator");
            if (string.Compare(useInProcDataGeneratorEnvValue, "true", StringComparison.OrdinalIgnoreCase) == 0) 
            {
                services.AddHostedService<InProcShelfDataGenerator>();
            }
            else 
            {
                services.AddHostedService<HttpModuleClient>();
            }
            
            /*Add ClientNotifier which will work with the ClientUpdateHub to send shelf data to the website*/
            services.AddHostedService<ClientNotifier>();            
            /*Use the Singleton design pattern, along with dependency injection in ClientNotifier, HttpModuleClient
              ,InProcShelfDataGenerator, and ClientUpdateHub (setup below)
            */
            services.AddSingleton<IBackgroundShelfQueue, BackgroundShelfQueue>();

            /*Add MVC*/
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            /*Add SignalR */
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            /*Boilerplate*/
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            /*boilerplate*/
            app.UseDefaultFiles();
            app.UseStaticFiles();

            /*Add SignalR route. Note- in ASP.NET Core, only one Hub is allowed 
              Note that in wwwroot, signalr is included (installed from npm) for the client to connect
            */
            app.UseSignalR(routes =>
            {
                routes.MapHub<ClientUpdateHub>("/clientUpdateHub");
            });

            /*boilerplate */
            app.UseMvc();
        }
    }
}
