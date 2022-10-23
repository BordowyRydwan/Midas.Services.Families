using Application.Interfaces;
using Application.Mappings;
using Application.Middlewares;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Midas.Services;
using NLog;
using NLog.Web;
using WebAPI.Extensions;

namespace WebAPI;

public class Startup
{
    private readonly WebApplicationBuilder _builder;
    private readonly Logger _logger;

    public Startup(string[] args)
    {
        _builder = WebApplication.CreateBuilder(args);
        _logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

        _logger.Debug("The Message API was started");
    }

    public Startup SetBuilderOptions()
    {
        _builder.Services.AddControllers();
        _builder.Services.AddEndpointsApiExplorer();
        _builder.Services.AddSwaggerGen(x =>
        {
            x.EnableAnnotations();
            x.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Families API", Version = "0.1" });
        });

        return this;
    }

    public Startup SetOpenCors()
    {
        _builder.Services.AddCors(options =>
        {
            options.AddPolicy("Open", builder => { builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
        });

        _logger.Debug("The CORS open policy was successfully added");

        return this;
    }

    public Startup SetDbContext()
    {
        var connString = _builder.Configuration.GetConnectionString("DefaultConnection");

        _builder.Services.AddDbContext<FamilyDbContext>(options =>
        {
            options.UseSqlServer(connString).EnableSensitiveDataLogging();
        });

        _logger.Debug("SQL connection was successfully added");

        return this;
    }

    public Startup SetMapperConfig()
    {
        var mapperConfig = AutoMapperConfig.Initialize();

        _builder.Services.AddSingleton(mapperConfig);
        _logger.Debug("The mapping config was successfully added");

        return this;
    }

    public Startup AddInternalServices()
    {
        _builder.Services.AddHttpContextAccessor();
        _builder.Services.AddScoped<IFamilyService, FamilyService>();
        _logger.Debug("Internal services were successfully added");

        return this;
    }

    public Startup AddInternalRepositories()
    {
        _builder.Services.AddScoped<IFamilyRepository, FamilyRepository>();
        _logger.Debug("Internal repositories were successfully added");

        return this;
    }

    public Startup AddLoggerConfig()
    {
        _builder.Logging.ClearProviders();
        _builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        _builder.Host.UseNLog();

        _logger.Debug("Logger options were successfully added");

        return this;
    }

    public Startup SetExternalServiceClients()
    {
        var authServiceAddress = _builder.Configuration["ServiceAddresses:User"];
        var httpClientDelegate = (Action<HttpClient>)(client => client.BaseAddress = new Uri(authServiceAddress));
        var httpClientHandler = new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, policyErrors) => true
        };    

        _builder.Services.AddHttpClient<IUserClient, UserClient>(httpClientDelegate)
            .ConfigurePrimaryHttpMessageHandler(() => httpClientHandler)
            .AddHeaderPropagation();
        
        return this;
    }

    public void Run()
    {
        var app = _builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHeaderPropagation();
        app.MigrateDatabase();
        app.UseHttpsRedirection();
        app.UseMiddleware<AuthorizationMiddleware>();
        app.UseCors("Open");
        app.UseAuthentication();
        app.MapControllers();
        app.Run();

        _logger.Debug("Application has been successfully ran");
    }
}