using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading.RateLimiting;

namespace RoomReservationApi.RateLimiting
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// What one caller is allowed per minute across the whole app. No named policy can exceed this, since
        /// every request has to pass this limiter as well as the policy of the endpoint it hit.
        /// </summary>
        public const int GlobalPermitLimit = 500;

        public const string RejectedResponseBody = @"{""message"":""Rate limit exceeded. Please try again later.""}";

        public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // Every named policy is partitioned per caller. An unpartitioned AddFixedWindowLimiter would be
                // one shared bucket for the whole app, letting a single caller exhaust a tier for everyone.
                AddPartitionedFixedWindowPolicy(options, RateLimitPolicies.Default, permitLimit: GlobalPermitLimit);
                AddPartitionedFixedWindowPolicy(options, RateLimitPolicies.Strict, permitLimit: 60);
                AddPartitionedFixedWindowPolicy(options, RateLimitPolicies.VeryStrict, permitLimit: 10);

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    // Both of these have to be set before anything is written to the body.
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);

                    // Every other error from this api is a json object with a message, so this one is too.
                    context.HttpContext.Response.ContentType = "application/json";

                    await context.HttpContext.Response.WriteAsync(RejectedResponseBody, cancellationToken);
                };

                // Per-caller floor across the whole app, on top of the route-specific named policies above. The
                // default policy is deliberately the same number: one endpoint is allowed to use up a caller's
                // whole allowance. Raising this leaves that endpoint's own limit in force.
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: RateLimitPartitionKey.Resolve(httpContext),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = GlobalPermitLimit,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                        }));
            });

            return services;
        }

        private static void AddPartitionedFixedWindowPolicy(RateLimiterOptions options, string policyName, int permitLimit)
        {
            options.AddPolicy(policyName, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: RateLimitPartitionKey.Resolve(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));
        }
    }
}
