using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nancy.Owin;

using Polly;

namespace ShoppingCart
{
    public class Startup
    {
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

            app.UseOwin(x => x.UseNancy());
        }

        //private async Task WaitForSqlAvailabilityAsync( ILoggerFactory loggerFactory, IApplicationBuilder app, int retries = 0)
        //{
        //    var logger = loggerFactory.CreateLogger(nameof(Startup));
        //    var policy = CreatePolicy(retries, logger, nameof(WaitForSqlAvailabilityAsync));
        //    await policy.ExecuteAsync(async () =>
        //    {

        //    });

        //}

        //private Policy CreatePolicy(int retries, ILogger logger, string prefix)
        //{
        //    return Policy.Handle<SqlException>().
        //        WaitAndRetryAsync(
        //            retryCount: retries,
        //            sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
        //            onRetry: (exception, timeSpan, retry, ctx) =>
        //            {
        //                logger.LogTrace($"[{prefix}] Exception {exception.GetType().Name} with message ${exception.Message} detected on attempt {retry} of {retries}");
        //            }
        //        );
        //}
    }
}
