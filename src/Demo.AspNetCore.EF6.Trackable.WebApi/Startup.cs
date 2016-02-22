using System;
using System.IO;
using Demo.AspNetCore.EF6.Trackable.Data.Contexts;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using System.Linq;

namespace Demo.AspNetCore.EF6.Trackable.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            // Set up data directory
            string appRoot = appEnv.ApplicationBasePath;
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(appRoot, "App_Data"));
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped(provider =>
            {
                var connectionString = Configuration["Data:NorthwindSlim:ConnectionString"];
                return new NorthwindSlim(connectionString);
            });

            // Add framework services
            services.AddMvc(options =>
            {
                // Preserve reference handling
                //foreach (var jsonOutput in options.OutputFormatters.OfType<JsonOutputFormatter>())
                //    jsonOutput.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.All;
                //foreach (var jsonInput in options.InputFormatters.OfType<JsonInputFormatter>())
                //    jsonInput.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.All;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseIISPlatformHandler();

            app.UseStaticFiles();

            app.UseMvc();
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
