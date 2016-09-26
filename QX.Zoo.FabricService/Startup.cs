using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using QX.Zoo.Runtime.Toys;
using QX.Zoo.Talk.MessageBus;
using QX.Zoo.Talk.RabbitMQ;

namespace QX.Zoo.FabricService
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddRabbitJsonFile();

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging()
                .AddSingleton<IAsyncBusBroker>(ctx => new RabbitMQBroker(Configuration.GetRabbitMQConfiguration(), new OperationScopeProvider(), ctx.GetService<ILoggerFactory>().CreateLogger("RabbitMQBroker")))
                .AddSingleton(ctx => ctx.GetRequiredService<IAsyncBusBroker>().GetEntity(Configuration["AsyncBus:ActionsEntity"]))
                .AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory
                .AddConsole(Configuration.GetSection("Logging"))
                .AddDebug();
            
            app.ServerFeatures.Set(loggerFactory);

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
                Audience = Configuration["Authentication:AzureAd:Audience"]
            });

            app.UseDeveloperExceptionPage();

            app.UseMvc();
        }
    }
}
