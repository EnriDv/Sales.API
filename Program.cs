using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Sales.API.Application.Interfaces;
using Sales.API.Application.Services;
using Sales.API.Application.Extensions;
using Sales.API.Infrastructure.Middleware;
using Sales.API.Infrastructure.Repositories;
using Scalar.AspNetCore;
using Sales.API.Infrastructure.Persistence;
using Sales.API.Shared.Middleware;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IKdsService, KdsService>();
builder.Services.AddScoped<ISalesDashboardService, SalesDashboardService>();
builder.Services.AddScoped<ITaxConfigurationService, TaxConfigurationService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<IWaiterService, WaiterService>();
builder.Services.AddInventoryApiClient(builder.Configuration);
builder.Services.AddScoped<ISalesCatalogService, SalesCatalogService>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});
var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins) 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();
app.UseForwardedHeaders();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSimpleIdempotency();


app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Sales API - ISW-312")
            .WithTheme(ScalarTheme.BluePlanet)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});



app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
