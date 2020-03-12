using System;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using WebApi.Filters;
using WebApi.Processor;

namespace WebApi
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
            services.Configure<IISOptions>(options => { options.AutomaticAuthentication = false; });

            var dbConnection = Configuration.GetConnectionString("Database");
            services.AddControllers();
            services.AddHangfire(x =>
            {
                var options = new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(30),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true,
                };
                x.UseSqlServerStorage(dbConnection, options);
            });

            services.AddScoped<IJobProcessor, JobProcessor>(provider =>
                new JobProcessor(Configuration.GetValue<string>("Url"), provider.GetService<ILoggerFactory>()));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WebApi Api",
                    Version = "v1"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi API V1");
                c.RoutePrefix = string.Empty;
            });

            //Configure HangFire
            app.UseHangfireDashboard("/dashboard", new DashboardOptions
            {
                Authorization = new[] { new CustomAuthorizeFilter() }
            });
            var interval = Configuration.GetValue<string>("Interval");
            //HangFire RecurringJob which runs as per the interval defined in settings file
            RecurringJob.AddOrUpdate<IJobProcessor>("MbsLive Web Scraping Job", x => x.Invoke(), interval, TimeZoneInfo.Local);
            app.UseHangfireServer();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}