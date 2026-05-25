using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using TheLeague.Shared.Contracts;
using TheLeague.Shared.Infrastructure;
using TheLeague.Shared.Infrastructure.Middleware;
using TheLeague.Shared.Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TheLeague")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Shared Infrastructure (tenancy, caching, messaging, behaviours, auth)
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Authentication (JWT)
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "ThisIsADevelopmentSecretKeyThatIsAtLeast64CharactersLongForSecurity!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TheLeague";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TheLeagueApp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };

    // SignalR token from query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "The League API",
        Version = "v1",
        Description = "Multi-tenant SaaS platform for sports club management",
        Contact = new OpenApiContact { Name = "The League", Email = "support@theleague.com" }
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks();

// Register all modules via IModule interface (assembly scanning)
var moduleTypes = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName?.StartsWith("TheLeague.Modules") == true)
    .SelectMany(a => a.GetTypes())
    .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
    .ToList();

foreach (var moduleType in moduleTypes)
{
    var module = (IModule)Activator.CreateInstance(moduleType)!;
    module.RegisterModule(builder.Services, builder.Configuration);
}

// MediatR (scan all module assemblies for handlers)
var moduleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName?.StartsWith("TheLeague.Modules") == true)
    .ToArray();

if (moduleAssemblies.Length > 0)
{
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(moduleAssemblies));
}

var app = builder.Build();

// Middleware pipeline (ORDER MATTERS)
app.UseExceptionHandling();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "The League API v1");
        c.RoutePrefix = "swagger";
    });
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "0");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    await next();
});

app.UseRateLimiting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantResolution();

// Use all registered modules
foreach (var moduleType in moduleTypes)
{
    var module = (IModule)Activator.CreateInstance(moduleType)!;
    module.UseModule(app);
}

app.MapControllers();
app.MapHealthChecks("/health");

// SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<MatchCentreHub>("/hubs/match-centre");

Log.Information("The League Platform started on {Urls}", string.Join(", ", app.Urls));

app.Run();
