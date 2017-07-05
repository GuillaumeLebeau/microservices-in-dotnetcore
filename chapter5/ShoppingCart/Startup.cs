using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using DbUp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var connectionString = Configuration["ConnectionString"];
            app.UseOwin(x => x.UseNancy(options => options.Bootstrapper = new Bootstrapper(connectionString, Configuration["EventStoreConnectionString"], Configuration["EventStoreType"])));

            WaitForSqlAvailabilityAsync(connectionString, loggerFactory, 2).Wait();
        }

        private async Task WaitForSqlAvailabilityAsync(string connectionString, ILoggerFactory loggerFactory, int retries = 0)
        {
            var logger = loggerFactory.CreateLogger(nameof(Startup));
            var policy = CreatePolicy(retries, logger, nameof(WaitForSqlAvailabilityAsync));
            await policy.ExecuteAsync(async () =>
            {
                EnsureDatabase.For.SqlDatabase(connectionString);

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    await conn.ExecuteAsync(@"IF NOT EXISTS (SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'shopcart')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA shopcart'
END").ConfigureAwait(false);
                }

                var upgrader = DeployChanges.To
                    .SqlDatabase(connectionString, "shopcart")
                    .WithScriptsEmbeddedInAssembly(Assembly.GetEntryAssembly())
                    .LogToConsole()
                    .Build();
                
                var result = upgrader.PerformUpgrade();

                if (!result.Successful)
                {
                    throw result.Error;
                }
            }).ConfigureAwait(false);
        }

        private Policy CreatePolicy(int retries, ILogger logger, string prefix)
        {
            return Policy.Handle<SqlException>().
                WaitAndRetryAsync(
                    retryCount: retries,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                        logger.LogTrace($"[{prefix}] Exception {exception.GetType().Name} with message ${exception.Message} detected on attempt {retry} of {retries}")
                );
        }
    }
}
