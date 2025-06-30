using FluentValidation;
using Microsoft.Extensions.Options;
using Serilog;
using System.Reflection;
using Vruchtgebruik.Api.Context;
using Vruchtgebruik.Api.Factories;
using Vruchtgebruik.Api.Interfaces;
using Vruchtgebruik.Api.Methods;
using Vruchtgebruik.Api.Middleware;
using Vruchtgebruik.Api.Settings;
using Vruchtgebruik.Api.Validators;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Vruchtgebruik.Api.Extensions;
[assembly: InternalsVisibleTo("Vruchtgebruik.IntegrationTests")]

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

// Register …ÈnLevenSettings with custom validation
builder.Services.AddValidatedSettings<EenLevenSettings>(
    builder.Configuration.GetSection("FactorMethods:EenLeven"),
    settings => !string.IsNullOrEmpty(settings.ActiveVersion)
                && settings.Versions != null
                && settings.Versions.Any(),
    "EenLevenSettings are not properly configured."
);

// Register AgeFactorSettings (example: no real validation)
builder.Services.AddValidatedSettings<AgeFactorSettings>(
    builder.Configuration.GetSection("AgeFactor"),
    settings => true
);

builder.Services.AddSingleton<IFactorCalculationMethod, EenLevenVruchtgebruikFactorMethod>();

builder.Services.AddSingleton<IFactorCalculationMethodFactory, FactorCalculationMethodFactory>();

builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();

builder.Services.AddValidatorsFromAssemblyContaining<CalculationRequestValidator>();

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("https://localhost:4200", "http://localhost:4200") // Your Angular dev URL
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddCustomRateLimiting();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(ApiExceptionHandler.Handle);
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS - before controllers/endpoints
app.UseCors();

app.UseAuthorization();

app.UseRateLimiter();

app.UseResponseCompression();

app.MapControllers();

// Custom 404 handler
app.Use(async (context, next) =>
{
    await next();
    await NotFoundHandler.Handle(context);
});

app.MapCustomHealthChecks();

app.Run();

public partial class Program { }