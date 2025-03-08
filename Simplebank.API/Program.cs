using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Simplebank.Application.Services;
using Simplebank.Domain.Interfaces.Database;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Interfaces.Services;
using Simplebank.Infrastructure.Database;
using Simplebank.Infrastructure.Repositories;

namespace Simplebank.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Configuration, builder.Services);
        var app = builder.Build();
        Configure(app);
        app.Run();
    }
    
    private static void ConfigureServices(ConfigurationManager configuration, IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddOpenApi();
        
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = false;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("Default"));
        });
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Repositories
        services.AddScoped<IAccountsRepository, AccountsRepository>();
        services.AddScoped<IEntriesRepository, EntriesRepository>();
        services.AddScoped<ITransfersRepository, TransfersRepository>();
        
        //Services
        services.AddScoped<IAccountsService, AccountsService>();
        services.AddScoped<IEntriesService, EntriesService>();
        services.AddScoped<ITransfersService, TransfersService>();
        
        services.AddControllers();
    }
    
    private static void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        
        app.UseAuthorization();
        app.MapControllers();
    }
}