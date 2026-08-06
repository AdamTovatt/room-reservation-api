using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RoomReservationApi.Helpers;
using RoomReservationApi.RateLimiting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoomReservationApi
{
    public class Startup
    {
        /// <summary>
        /// How long a fetched schedule is served from memory before it is fetched again, which is also the
        /// shortest possible interval between two outbound requests to the KTH api for the same day.
        /// </summary>
        private static readonly TimeSpan scheduleCacheLifetime = TimeSpan.FromMinutes(1);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddControllers().AddNewtonsoftJson();

            services.AddRateLimitingServices();

            string apiKey = EnvironmentHelper.GetEnvironmentVariable("KTH_API_KEY");

            IHttpClientBuilder httpClientBuilder = services.AddHttpClient("ApiClient");
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(serviceProvider => HttpClientHelper.CreateHandler());

            // The cache is shared by every request, so it has to be a singleton for it to have any effect.
            services.AddSingleton<ScheduleCache>(serviceProvider => new ScheduleCache(scheduleCacheLifetime));

            services.AddScoped<ApiService>(serviceProvider =>
            {
                IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = httpClientFactory.CreateClient("ApiClient");
                ScheduleCache scheduleCache = serviceProvider.GetRequiredService<ScheduleCache>();
                ILogger<ApiService> logger = serviceProvider.GetRequiredService<ILogger<ApiService>>();
                return new ApiService(httpClient, apiKey, scheduleCache, logger);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Patch path base with forwarded path
            app.Use(async (context, next) =>
            {
                string? forwardedPath = context.Request.Headers["X-Forwarded-Path"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedPath))
                {
                    context.Request.PathBase = forwardedPath;
                }

                await next();
            });

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RoomReservationApi v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseRateLimiter(); // after UseRouting, so the policy of the matched endpoint is known

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
