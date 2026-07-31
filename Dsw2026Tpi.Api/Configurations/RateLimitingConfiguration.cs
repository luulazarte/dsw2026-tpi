using Dsw2026Tpi.CrossCutting.Models;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations
{
    public static class RateLimitingConfiguration
    {
        public static void AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Leemos las configuraciones desde el appsettings.json (no quedan hardcodeadas)
            var adminLimit = configuration.GetValue<int>("RateLimitingSettings:AdminAuthLimit", 5);
            var patientLimit = configuration.GetValue<int>("RateLimitingSettings:PatientAuthLimit", 10);
            var bookingLimit = configuration.GetValue<int>("RateLimitingSettings:BookingLimit", 5);
            var globalLimit = configuration.GetValue<int>("RateLimitingSettings:GlobalLimit", 100);

            services.AddRateLimiter(options =>
            {
                // Código 429 como pide el TPI
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Manejador cuando se rechaza una solicitud
                options.OnRejected = async (context, token) =>
                {
                    // A. Registrar el rechazo mediante el log del sistema
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Rate limit superado. IP: {Ip}, Endpoint: {Endpoint}",
                        context.HttpContext.Connection.RemoteIpAddress,
                        context.HttpContext.Request.Path);

                    // B. Respetar el formato general de errores definido (tu ErrorResponse)
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    // Podés agregar "TOO_MANY_REQUESTS" a tus ErrorCodes en un futuro si querés
                    var errorResponse = new ErrorResponse("TOO_MANY_REQUESTS", "Se ha superado el límite de solicitudes permitidas.");

                    var json = JsonSerializer.Serialize(errorResponse);
                    await context.HttpContext.Response.WriteAsync(json, token);
                };

                // Política 1: Autenticación Admin (5 req / minuto / IP)
                options.AddPolicy("AdminAuthPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = adminLimit,
                            QueueLimit = 0, // No encolar
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // Política 2: Autenticación Paciente (10 req / minuto / IP)
                options.AddPolicy("PatientAuthPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = patientLimit,
                            QueueLimit = 0, // No encolar
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // Política 3: Reserva de turnos (5 req / minuto / Paciente Autenticado)
                options.AddPolicy("BookingPolicy", httpContext =>
                {
                    // Extraemos el ID del usuario del token JWT, si no hay, caemos a la IP
                    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                 ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                 ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: userId,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = bookingLimit,
                            QueueLimit = 0, // No encolar
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // Política Global: Restantes endpoints (100 req / minuto / Usuario o IP)
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
                            QueueLimit = 0, // No encolar
                            Window = TimeSpan.FromMinutes(1)
                        });
                });
            });
        }
    }
}
