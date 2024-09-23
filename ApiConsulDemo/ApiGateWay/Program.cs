
using Microsoft.Extensions.Hosting.Internal;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

namespace ApiGateWay
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

           
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
            //.AddJsonFile("appsettings.json")
            //.AddJsonFile("ocelot.json");
            //builder.Services.AddOcelot().AddConsul();

            builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile("Ocelot.json");

            builder.Services.AddHealthChecks();
            builder.Services.AddOcelot().AddConsul()                
                .AddConfigStoredInConsul();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("clients-allowed", opts =>
                {
                    opts.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();          


            app.MapControllers();
            app.UseCors("clients-allowed");
            app.UseOcelot().Wait();
            app.Run();
        }
    }
}
