using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using simple_rank_backend.Application.Middleware;
using simple_rank_backend.Application.Services;
using Supabase;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Newtonsoft.Json;
using simple_rank_backend.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using simple_rank_backend.Application.Configuration;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day));


// Add services to the container.
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

var supabaseUrl = Environment.GetEnvironmentVariable("supabaseUrl", EnvironmentVariableTarget.Machine);
var supabaseKey = Environment.GetEnvironmentVariable("supabaseServiceKey", EnvironmentVariableTarget.Machine);
var supabaseJwtSecret = Environment.GetEnvironmentVariable("supabaseJwt", EnvironmentVariableTarget.Machine); // The JWT Secret from Supabase Auth settings

if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
{
    throw new ArgumentNullException("Supabase URL or Key is not set in environment variables.");
}

if (string.IsNullOrEmpty(supabaseJwtSecret))
{
    throw new ArgumentNullException("supabaseJwt is not set in environment variables. This is required for JWT validation.");
}


if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
{
    throw new ArgumentNullException("Supabase URL or Key is not set in environment variables.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    Console.WriteLine($"{supabaseUrl}/auth/v1");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseJwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = $"{supabaseUrl}/auth/v1",

        ValidateAudience = true,
        ValidAudience = "authenticated",

        ValidateLifetime = true,
        NameClaimType = ClaimTypes.Email,
        RoleClaimType = "role",
    };

    // Optional: Add logging for debugging authentication failures
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Log the exception message. This will give you more details on why auth failed.
            Console.WriteLine("Authentication failed: " + context.Exception.Message);
            // Example: If you see 'IDX10241: Security token signature key not found' or 'IDX10503: Signature validation failed',
            // it points to an issue with IssuerSigningKey.
            // 'IDX10222: Lifetime validation failed. The token is expired.' means the token lifetime is the issue.
            // 'IDX10208: Unable to obtain configuration from: '[WellKnownFigPath]' if Authority was used and discovery failed.
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token successfully validated for: " + context?.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
});

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminPolicy", policy =>
//        policy.RequireClaim(ClaimTypes.Role, "Admin"));
//    options.AddPolicy("UserPolicy", policy =>
//        policy.RequireClaim(ClaimTypes.Role, "User", "Admin"));
//});

builder.Services.AddMemoryCache( options => { options.SizeLimit = 1000; });
builder.Services.Configure<UserCacheOptions>(builder.Configuration.GetSection("UserCache"));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddScoped<Client>(_ => 
    new Client(supabaseUrl, supabaseKey, 
        new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
            //StorageClientOptions = new Supabase.Storage.ClientOptions()
        }
    ));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IRankService, RankingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();
builder.Services.AddSingleton<IUserCacheService, UserCacheService>();

builder.Services.AddRateLimiter(options =>
{
    // Fixed window rate limiter - 100 requests per minute
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 20;
    });

    // Sliding window rate limiter - 50 requests per 30 seconds
    options.AddSlidingWindowLimiter("sliding", limiterOptions =>
    {
        limiterOptions.PermitLimit = 50;
        limiterOptions.Window = TimeSpan.FromSeconds(30);
        limiterOptions.SegmentsPerWindow = 6;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });

    // Per-user rate limiter - 20 requests per minute per user
    options.AddTokenBucketLimiter("per-user", limiterOptions =>
    {
        limiterOptions.TokenLimit = 20;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
        limiterOptions.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        limiterOptions.TokensPerPeriod = 20;
        limiterOptions.AutoReplenishment = true;
    });

    // Global rate limiter by IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", token);
    };
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontEnd",policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://simple-rank.com", "https://api.simple-rank.com", "https://localhost:7123", "http://localhost:5033")
              .AllowAnyMethod()
              .AllowCredentials()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontEnd");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}


app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
    };
});


app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<AuthTokenValidator>();
app.MapControllers();

app.Run();
