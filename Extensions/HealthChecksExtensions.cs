using ExternalServices.Gateway.Api.Infrastructure.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace ExternalServices.Gateway.Api.Extensions
{
    public static class HealthChecksExtensions
    {
        public static IHealthChecksBuilder RegisterHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var builder = services.AddHealthChecks();

            builder.AddCheck<MemoryHealthCheck>("Memory");
            builder.AddCheck<ApiHealthCheck>("ApiHealth");
            builder.AddUrlGroup(new Uri("https://localhost:5111/WeatherForecast"), "weatherForecast");
            builder.AddPingHealthCheck(_ =>
            {
                _.AddHost("google.com", 200); // _.AddHost("172.217.17.196", 200);
            }, "Google ping Test");
            builder.AddCheck("Google ping Test 2", new PingHealthCheck("google.com", 10));
            builder.AddCheck("Google ping Test 3", new PingHealthCheck("google.com", 200));
            builder.AddCheck("Google ping Test 4", new PingHealthCheck("googleamca.com", 200));
            builder.AddSmtpHealthCheck(_ =>
            {
                _.Host = "smtp.gmail.com";
                _.Port = 25;
                _.ConnectionType = HealthChecks.Network.Core.SmtpConnectionType.TLS;
            }, "Gmail SMTP Ping");
            builder.AddDiskStorageHealthCheck(s => s.AddDrive("C:\\", 1024)); // 1024 MB (1 GB) free minimum
            builder.AddVirtualMemorySizeHealthCheck(512); // 512 MB max allocated memory
            builder.AddPrivateMemoryHealthCheck(512); // 512 MB max allocated memory
            builder.AddProcessHealthCheck("chrome.exe", p => p.Length > 0); // check if process is running

            services.AddHealthChecksUI(setupSettings: setup =>
            {
                setup.SetEvaluationTimeInSeconds(5);
                setup.SetHeaderText("Health Checks Status");
                setup.DisableDatabaseMigrations();
                setup.MaximumHistoryEntriesPerEndpoint(50);
                setup.AddHealthCheckEndpoint("ExternalServices Gateway Api", "https://localhost:5111/healthchecks");
                setup.AddWebhookNotification("Slack Notification WebHook", "Your_Slack_WebHook_Uri_Goes_Here",
                                              "{\"text\": \"[[LIVENESS]] is failing with the error message : [[FAILURE]]\"}",
                                              "{\"text\": \"[[LIVENESS]] is recovered.All is up & running !\"}");

            }).AddInMemoryStorage();

            return builder;
        }
    }
}
