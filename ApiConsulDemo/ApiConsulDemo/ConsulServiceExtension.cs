namespace ApiConsulDemo
{
    using System;

    using System.Net;
    using Consul;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    namespace SampleAPI
    {
        public static class ConsulServiceExtension
        {
            public static IServiceCollection AddConsulConfig(this IServiceCollection services, IConfiguration configuration)
            {
                services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
                {
                    var host = configuration.GetValue<string>("ConsulConfig:ConsulHost");
                    consulConfig.Address = new Uri(host);
                }));

                return services;
            }

            public static IApplicationBuilder UseConsul(this IApplicationBuilder app, IConfiguration configuration)
            {
                var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
                var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("AppExtensions");
                var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

                if (app.Properties["server.Features"] is not FeatureCollection features)
                {
                    return app;
                }



                var servicePort = int.Parse(configuration.GetValue<string>("ServiceConfiguration:ServicePort"));
                //var serviceHost = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                var serviceHost = configuration.GetValue<string>("ServiceConfiguration:ServiceHost");
                var serviceName = configuration.GetValue<string>("ServiceConfiguration:ServiceName");
                var serviceId = serviceName;// + "-" + Guid.NewGuid();


                //IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                //var localIp = "192.168.1.103";
                var registration = new AgentServiceRegistration()
                {
                    ID = serviceId,
                    Name = serviceName,
                    Address = serviceHost,
                    //Address= localIp,
                    Port = servicePort,
                    Tags = ["sampleapi_tag"],

                    //Check = new AgentCheckRegistration()
                    //{
                    //    HTTP = $"http://{serviceHost}:{servicePort}/health",
                    //    Interval = TimeSpan.FromSeconds(10)
                    //}
                };

                logger.LogInformation("Registering with Consul");
                consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
                consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

                lifetime.ApplicationStopping.Register(() =>
                {
                    logger.LogInformation("Unregistering from Consul");
                    consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
                });

                return app;
            }
        }
    }
}
