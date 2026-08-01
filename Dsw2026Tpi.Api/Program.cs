using Dsw2026Tpi.Api.Configurations;
using Dsw2026Tpi.Api.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Dsw2026Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddSerilogConfiguration();

        builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(
                            new System.Text.Json.Serialization.JsonStringEnumConverter());
                    }); builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddApplicationPersistence(builder.Configuration);
        builder.Services.AddAppIdentity();
        builder.Services.AddAppAuthentication(builder.Configuration);
        builder.Services.AddAppCors(builder.Configuration);
        builder.Services.AddAppDependencies();
        builder.Services.AddSwaggerConfiguration();

        builder.Services.AddAppRateLimiting(builder.Configuration);

        builder.Services.AddHealthChecks();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var domainDb = services.GetRequiredService<Dsw2026Tpi.Data.Dsw2026TpiDbContext>();
            await domainDb.Database.MigrateAsync();

            var authDb = services.GetRequiredService<Dsw2026Tpi.Data.Identity.AuthenticationDbContext>();
            await authDb.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<Dsw2026Tpi.Data.Identity.ApplicationUser>>();

            const string adminRole = "Administrador";
            const string adminEmail = "admin@system.com";
            const string adminPassword = "Admin1234";

            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole(adminRole));

            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new Dsw2026Tpi.Data.Identity.ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var creado = await userManager.CreateAsync(admin, adminPassword);

                if (!creado.Succeeded)
                {
                    var errores = string.Join(" | ", creado.Errors.Select(e => e.Description));
                    throw new Exception("No se pudo crear el admin: " + errores);
                }

                await userManager.AddToRoleAsync(admin, adminRole);
            }
        }
        

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (app.Environment.IsProduction())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        app.MapControllers();
        app.MapHealthChecks("/health-check");

        app.Run();
    }
}
