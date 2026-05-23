using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.API.Application.Interfaces;
using Sales.API.Application.Services;

namespace Sales.API.Application.Extensions;

public static class ServiceCollectionExtensions
{
    private const string InventoryApiConfigKey = "ServiceUrls:InventoryAPI";

    public static IServiceCollection AddInventoryApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IInventoryApiClient, InventoryApiClient>(client =>
        {
            var inventoryApiBaseUrl = configuration[InventoryApiConfigKey]
                ?? throw new InvalidOperationException($"Missing configuration value '{InventoryApiConfigKey}'.");
            client.BaseAddress = new Uri(inventoryApiBaseUrl.TrimEnd('/') + "/");
        });

        return services;
    }
}