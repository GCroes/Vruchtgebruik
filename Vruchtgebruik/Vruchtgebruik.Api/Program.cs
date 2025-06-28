using FluentValidation;
using Microsoft.Extensions.Options;
using Serilog;
using Vruchtgebruik.Api.Context;
using Vruchtgebruik.Api.Factories;
using Vruchtgebruik.Api.Interfaces;
using Vruchtgebruik.Api.Methods;
using Vruchtgebruik.Api.Middleware;
using Vruchtgebruik.Api.Settings;
using Vruchtgebruik.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Read configuration from appsettings.json and setup Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

// Configure settings as before
builder.Services.Configure<EenLevenSettings>(builder.Configuration.GetSection("FactorMethods:EenLeven"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<EenLevenSettings>>().Value);

builder.Services.Configure<AgeFactorSettings>(builder.Configuration.GetSection("AgeFactor"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AgeFactorSettings>>().Value);

builder.Services.AddSingleton<IFactorCalculationMethod, EenLevenVruchtgebruikFactorMethod>();

builder.Services.AddSingleton<IFactorCalculationMethodFactory, FactorCalculationMethodFactory>();

builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();

builder.Services.AddValidatorsFromAssemblyContaining<CalculationRequestValidator>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
