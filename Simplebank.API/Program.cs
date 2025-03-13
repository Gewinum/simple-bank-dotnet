using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NaCl.Core;
using Org.BouncyCastle.Utilities;
using Paseto.Cryptography;
using Simplebank.API.Authorization;
using Simplebank.Application.Mapping;
using Simplebank.Application.Providers;
using Simplebank.Application.Services;
using Simplebank.Domain.Interfaces.Database;
using Simplebank.Domain.Interfaces.Providers;
using Simplebank.Domain.Interfaces.Repositories;
using Simplebank.Domain.Interfaces.Services;
using Simplebank.Domain.Models.Users;
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
        services.AddPasetoAuthentication("Paseto", _ => { });
        
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

        services.AddAutoMapper(m => m.AddProfile<MappingProfile>());
        
        // Repositories
        services.AddScoped<IAccountsRepository, AccountsRepository>();
        services.AddScoped<IEntriesRepository, EntriesRepository>();
        services.AddScoped<ITransfersRepository, TransfersRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        
        // Providers
        services.AddScoped<IPasswordsProvider, PasswordsProvider>();
        
        var tokenPrivateKey = configuration.GetSection("Tokens").GetValue<string>("PrivateKey");
        
        if (string.IsNullOrWhiteSpace(tokenPrivateKey))
        {
            throw new ArgumentException("Token private key is not set");
        }

        if (tokenPrivateKey.Length != 32)
        {
            throw new ArgumentException("Token private key must be 32 bytes long");
        }

        services.AddSingleton<ITokensProvider>(new TokensProvider(Encoding.ASCII.GetBytes(tokenPrivateKey)));
        
        //Services
        services.AddScoped<IAccountsService, AccountsService>();
        services.AddScoped<IEntriesService, EntriesService>();
        services.AddScoped<ITransfersService, TransfersService>();
        services.AddScoped<IUsersService, UsersService>();
        
        services.AddControllers();
    }
    
    private static void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}