using System.Reflection;
using Application;
using Application.Common.Interfaces;
using Application.Options;
using CodeSigningApi.Extensions;
using CodeSigningApi.Services;
using Infrastructure;
using Infrastructure.Middlewares;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((_, options) =>
{
    options.Limits.MaxRequestBodySize = Int64.MaxValue;
});

if (!Directory.Exists(SigningOptions.tempFolder))
{
    Directory.CreateDirectory(SigningOptions.tempFolder);
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
    loggingBuilder.AddSerilog());

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

builder.Services.Configure<FormOptions>(option =>
{
    option.ValueLengthLimit = int.MaxValue;
    option.MultipartBodyLengthLimit = int.MaxValue;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Code Signing API",
        Version = "v1",
        Description = "API for signing files with a Code Signing Signature.<br/><br/>" +
                      "The API is protected by a JWT token. To obtain a token, use the `/api/Settings/CreateAuthToken` endpoint.<br/>" +
                      "The token must be sent in the Authorization header with the Bearer scheme.<br/><br/>" +
                      "The API is also protected by an IP whitelist. To add an IP to the whitelist, use the `/api/Settings/AllowIP` or `/api/Settings/AllowIPs` or `/api/Settings/AllowIPsFile` endpoints.<br/><br/>" +
                      "Both the authentication and the IP whitelist can be disabled in the `appsettings`.<br/><br/><br/>" +
                      "Each endpoint has its own Swagger documentation. To access the Swagger documentation, click on the endpoint and then click on the 'Try it out' button.",
        Contact = new OpenApiContact
        {
            Name = "Karbust",
            Url = new Uri("https://github.com/Karbust/CodeSigningApi")
        }
    });
    
    option.EnableAnnotations();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    option.IncludeXmlComments(xmlPath);
    
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "Base64",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddCustomCache(builder.Configuration);

builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddPersistenceServices(builder.Configuration)
    .AddApplicationServices();

builder.Services.AddOptions<SettingOptions>()
    .BindConfiguration(SettingOptions.Configuration)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.Configure<SigningOptions>(builder.Configuration.GetSection(SigningOptions.Configuration));

builder.Services.AddTransient<ICurrentSessionService, CurrentSessionService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseMiddleware<SerilogHttpMiddleware>();
app.UseCustomExceptionHandler();

if (app.Environment.IsDevelopment() ||
    builder.Configuration.GetSection("CodeSigningAPI:EnableSwagger").Get<bool>())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{ 
    app.UseHttpsRedirection(); 
}

app.UseForwardedHeaders();

app.ApplyMigrations();
await app.ApplyCache();

app.UseIPWhitelist();

app.UseRouting();

app.MapControllers();

app.Run();