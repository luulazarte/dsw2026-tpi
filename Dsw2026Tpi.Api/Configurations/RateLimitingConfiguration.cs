using Dsw2026Tpi.CrossCutting.Models;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations
{
    public static class RateLimitingConfiguration
    {
        public static void AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            
            var adminLimit = configuration.GetValue<int>("RateLimitingSettings:AdminAuthLimit", 5);
            var patientLimit = configuration.GetValue<int>("RateLimitingSettings:PatientAuthLimit", 10);
            var bookingLimit = configuration.GetValue<int>("RateLimitingSettings:BookingLimit", 5);
            var globalLimit = configuration.GetValue<int>("RateLimitingSettings:GlobalLimit", 100);

            services.AddRateLimiter(options =>
            {
                
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                
                options.OnRejected = async (context, token) =>
                {
                    
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Rate limit superado. IP: {Ip}, Endpoint: {Endpoint}",
                        context.HttpContext.Connection.RemoteIpAddress,
                        context.HttpContext.Request.Path);

                    
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    
                    var errorResponse = new ErrorResponse("TOO_MANY_REQUESTS", "Se ha superado el límite de solicitudes permitidas.");

                    var json = JsonSerializer.Serialize(errorResponse);
                    await context.HttpContext.Response.WriteAsync(json, token);
                };

                
                options.AddPolicy("AdminAuthPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = adminLimit,
                            QueueLimit = 0, 
                            Window = TimeSpan.FromMinutes(1)
                        }));

                
                options.AddPolicy("PatientAuthPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = patientLimit,
                            QueueLimit = 0, 
                            Window = TimeSpan.FromMinutes(1)
                        }));

                
                options.AddPolicy("BookingPolicy", httpContext =>
                {
                    
                    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                 ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                 ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: userId,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = bookingLimit,
                            QueueLimit = 0, 
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var partitionKey = httpContext.User.Identity?.IsAuthenticated == true
                        ? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown-user"
                        : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = globalLimit,
                            QueueLimit = 0, 
                            Window = TimeSpan.FromMinutes(1)
                        });
                });
            });
        }
    }
}
