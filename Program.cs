using ExternalServices.Gateway.Api.Infrastructure.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalServices.Gateway.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Information()
            //    .WriteTo.Console()
            //    .WriteTo.Debug(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
            //                   outputTemplate: DateTime.Now.ToString())
            //    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            //    .WriteTo.Seq("http://localhost:5341/")
            //   .WriteTo.Debug()
            //   .MinimumLevel.Override("Microsoft.AspNetCore",LogEventLevel.Warning)
            //   .Enrich.WithProperty("AppName","ExternalServices.Gateway.Api")
            //   .Enrich.WithProperty("Environment", "development")
            //   .Enrich.With(new ThreadIdEnricher())
            //   .Enrich.WithThreadId()
            //   .Enrich.WithHttpRequestId()
            //   .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
               Host.CreateDefaultBuilder(args)
                   .ConfigureWebHostDefaults(webBuilder =>
                   {
                       webBuilder
                           .UseUrls("https://localhost:5111")
                           .UseStartup<Startup>();
                   })
                  .ConfigureLogging(config => config.ClearProviders())
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true);
                })
                  .UseSerilog((hostingContext, loggerConfiguration) =>
                    loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
                    .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment)
                    .Enrich.With(new ThreadIdEnricher())
                  );
    }
}
