using Microsoft.EntityFrameworkCore;
using Sales.API.Application.Interfaces;
using Sales.API.Application.Services;
using Sales.API.Application.Extensions;
using Sales.API.Infrastructure.Persistence;
using Sales.API.Infrastructure.Middleware;
using Sales.API.Infrastructure.Repositories;
using Scalar.AspNetCore;
using Shared.Core.Middleware;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSimpleIdempotency();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Sales API - ISW-312")
               .WithTheme(ScalarTheme.BluePlanet)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}



app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();