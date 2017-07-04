using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using DbUp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nancy.Owin;

using Polly;

namespace ShoppingCart
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"settings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOwin(x => x.UseNancy(options => options.Bootstrapper = new Bootstrapper(Configuration)));

            WaitForSqlAvailabilityAsync(loggerFactory, app, 2);
        }

        private void WaitForSqlAvailabilityAsync(ILoggerFactory loggerFactory, IApplicationBuilder app, int retries = 0)
        {
            var logger = loggerFactory.CreateLogger(nameof(Startup));
            var policy = CreatePolicy(retries, logger, nameof(WaitForSqlAvailabilityAsync));
            policy.Execute(() =>
            {
                EnsureDatabase.For.SqlDatabase(Configuration["ConnectionString"]);

                var upgrader = DeployChanges.To
                    .SqlDatabase(Configuration["ConnectionString"])
                    .WithScriptsEmbeddedInAssembly(Assembly.GetEntryAssembly())
                    .LogToConsole()
                    .Build();
                
                var result = upgrader.PerformUpgrade();

                if (!result.Successful)
                {
                    throw result.Error;
                }
            });
        }

        private Policy CreatePolicy(int retries, ILogger logger, string prefix)
        {
            return Policy.Handle<SqlException>().
                WaitAndRetry(
                    retryCount: retries,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        logger.LogTrace($"[{prefix}] Exception {exception.GetType().Name} with message ${exception.Message} detected on attempt {retry} of {retries}");
                    }
                );
        }
    }
}
