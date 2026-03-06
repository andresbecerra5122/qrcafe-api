using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;
using MediatR;
using QrCafe.Application;
using QrCafe.Api.Middlewares;
using Npgsql;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Api.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<AdminBootstrapper>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

var cs = builder.Configuration.GetConnectionString("Default");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(cs);

//dataSourceBuilder.MapEnum<order_type>("order_type");
//dataSourceBuilder.MapEnum<order_status>("order_status");
//dataSourceBuilder.MapEnum<PaymentProvider>("payment_provider");
//dataSourceBuilder.MapEnum<PaymentStatus>("payment_status");

var dataSource = dataSourceBuilder.Build();

if (builder.Environment.IsDevelopment())
    Console.WriteLine("ConnectionString: " + cs);

builder.Services.AddDbContext<QrCafeDbContext>(opt =>
{
    opt.UseNpgsql(dataSource);
});

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var jwtSecretFromEnv = builder.Configuration["JWT_SECRET"];
var jwtIssuerFromEnv = builder.Configuration["JWT_ISSUER"];
var jwtAudienceFromEnv = builder.Configuration["JWT_AUDIENCE"];
var jwtExpiryFromEnv = builder.Configuration["JWT_EXPIRY_MINUTES"];

if (!string.IsNullOrWhiteSpace(jwtSecretFromEnv))
{
    jwtOptions.Secret = jwtSecretFromEnv;
}

if (!string.IsNullOrWhiteSpace(jwtIssuerFromEnv))
{
    jwtOptions.Issuer = jwtIssuerFromEnv;
}

if (!string.IsNullOrWhiteSpace(jwtAudienceFromEnv))
{
    jwtOptions.Audience = jwtAudienceFromEnv;
}

if (int.TryParse(jwtExpiryFromEnv, out var expiryMinutes) && expiryMinutes > 0)
{
    jwtOptions.ExpiryMinutes = expiryMinutes;
}

builder.Services.Configure<JwtOptions>(options =>
{
    options.Secret = jwtOptions.Secret;
    options.Issuer = jwtOptions.Issuer;
    options.Audience = jwtOptions.Audience;
    options.ExpiryMinutes = jwtOptions.ExpiryMinutes;
});
if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
{
    throw new InvalidOperationException("Jwt:Secret must be configured and at least 32 characters long.");
}

var jwtKey = Encoding.UTF8.GetBytes(jwtOptions.Secret);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthConstants.PolicyStaffAny, policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy(AuthConstants.PolicyAdminOnly, policy =>
    {
        policy.RequireRole(
            StaffRole.Admin.ToString(),
            StaffRole.Manager.ToString()
        );
    });

    options.AddPolicy(AuthConstants.PolicyKitchenOrAdmin, policy =>
    {
        policy.RequireRole(
            StaffRole.Admin.ToString(),
            StaffRole.Manager.ToString(),
            StaffRole.Kitchen.ToString()
        );
    });

    options.AddPolicy(AuthConstants.PolicyWaiterOrAdmin, policy =>
    {
        policy.RequireRole(
            StaffRole.Admin.ToString(),
            StaffRole.Manager.ToString(),
            StaffRole.Waiter.ToString()
        );
    });
});

var allowedOrigins = builder.Configuration.GetValue<string>("ALLOWED_ORIGINS")
    ?? "http://localhost:4200,http://localhost:4201";

builder.Services.AddCors(opt =>
{
    if (builder.Environment.IsDevelopment())
    {
        opt.AddPolicy("AllowAngular", p =>
            p.SetIsOriginAllowed(_ => true)
             .AllowAnyHeader()
             .AllowAnyMethod()
        );
    }
    else
    {
        opt.AddPolicy("AllowAngular", p =>
            p.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
             .AllowAnyHeader()
             .AllowAnyMethod()
        );
    }
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var bootstrapper = scope.ServiceProvider.GetRequiredService<AdminBootstrapper>();
    await bootstrapper.RunAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
