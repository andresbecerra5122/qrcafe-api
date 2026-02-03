using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;
using MediatR;
using QrCafe.Application;
using QrCafe.Api.Middlewares;
using Npgsql;
using QrCafe.Domain.Entities.Enums;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

var cs = builder.Configuration.GetConnectionString("Default");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(cs);

//dataSourceBuilder.MapEnum<order_type>("order_type");
//dataSourceBuilder.MapEnum<order_status>("order_status");
//dataSourceBuilder.MapEnum<PaymentProvider>("payment_provider");
//dataSourceBuilder.MapEnum<PaymentStatus>("payment_status");

var dataSource = dataSourceBuilder.Build();

Console.WriteLine("ConnectionString: " + cs);

builder.Services.AddDbContext<QrCafeDbContext>(opt =>
{
    opt.UseNpgsql(dataSource);
});

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAngular", p =>
        p.WithOrigins("http://localhost:4200")
         .AllowAnyHeader()
         .AllowAnyMethod()
    );
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ApiExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
