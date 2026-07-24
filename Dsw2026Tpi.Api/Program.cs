using Dsw2026Tpi.Api.Configurations;
using Dsw2026Tpi.Api.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;

namespace Dsw2026Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Database=DswTpi;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30";
      

        builder.AddSerilogConfiguration();

       
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        
        builder.Services.AddApplicationPersistence(builder.Configuration); 
        builder.Services.AddAppIdentity();                                 
        builder.Services.AddAppAuthentication(builder.Configuration);     
        builder.Services.AddAppCors(builder.Configuration);                 
        builder.Services.AddAppDependencies();                              
        builder.Services.AddSwaggerConfiguration();                       

        builder.Services.AddHealthChecks();

        var app = builder.Build();

 
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

        app.MapControllers();
        app.MapHealthChecks("/health-check");

        app.Run();
    }
}
