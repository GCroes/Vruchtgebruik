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
            .WithOrigins("https://localhost:4200") // Your Angular dev URL
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

app.UseAuthorization();

app.MapControllers();

// Custom 404 handler
app.Use(async (context, next) =>
{
    await next();
    await NotFoundHandler.Handle(context);
});

app.Run();

public partial class Program { }