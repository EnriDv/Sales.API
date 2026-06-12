using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Sales.API.Shared.Cen;
using Sales.API.Shared.Exceptions;
using Polly;

namespace Sales.API.Application.Services;

public class InventoryApiClient : IInventoryApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _uow;

    private static readonly IAsyncPolicy<List<SellableProductContractDto>> SellableProductsPolicy;
    private static readonly IAsyncPolicy<Dictionary<string, ProductLookupContractDto>> ProductLookupPolicy;
    private static readonly IAsyncPolicy ConsumeStockPolicy;

    static InventoryApiClient()
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
            );

        var circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );

        var fallbackSellableProducts = Policy<List<SellableProductContractDto>>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<Polly.CircuitBreaker.BrokenCircuitException>()
            .FallbackAsync((cancellationToken) => Task.FromException<List<SellableProductContractDto>>(new ConflictException("Inventario no disponible temporalmente")));

        SellableProductsPolicy = Policy.WrapAsync<List<SellableProductContractDto>>(
            fallbackSellableProducts,
            retryPolicy.AsAsyncPolicy<List<SellableProductContractDto>>(),
            circuitBreaker.AsAsyncPolicy<List<SellableProductContractDto>>()
        );

        var fallbackProductLookup = Policy<Dictionary<string, ProductLookupContractDto>>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<Polly.CircuitBreaker.BrokenCircuitException>()
            .FallbackAsync((cancellationToken) => Task.FromException<Dictionary<string, ProductLookupContractDto>>(new ConflictException("Inventario no disponible temporalmente")));

        ProductLookupPolicy = Policy.WrapAsync<Dictionary<string, ProductLookupContractDto>>(
            fallbackProductLookup,
            retryPolicy.AsAsyncPolicy<Dictionary<string, ProductLookupContractDto>>(),
            circuitBreaker.AsAsyncPolicy<Dictionary<string, ProductLookupContractDto>>()
        );

        var fallbackConsumeStock = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<Polly.CircuitBreaker.BrokenCircuitException>()
            .FallbackAsync((cancellationToken) => Task.FromException(new ConflictException("Inventario no disponible temporalmente")));

        ConsumeStockPolicy = Policy.WrapAsync(
            fallbackConsumeStock,
            retryPolicy,
            circuitBreaker
        );
    }

    public InventoryApiClient(HttpClient httpClient, IUnitOfWork uow)
    {
        _httpClient = httpClient;
        _uow = uow;
    }

    public async Task<List<SellableProductContractDto>> GetSellableProductsAsync(
        string companyCen,
        string? search = null,
        string? categoryCen = null,
        string? warehouseCen = null,
        bool onlyAvailable = true,
        int page = 1,
        int pageSize = 50)
    {
        return await SellableProductsPolicy.ExecuteAsync(async () =>
        {
            var effectiveWarehouseCen = await ResolveWarehouseCenAsync(companyCen, warehouseCen);
            var requestPath = BuildSellableProductsPath(companyCen, search, categoryCen, effectiveWarehouseCen, onlyAvailable);
            using var response = await _httpClient.GetAsync(requestPath);

            if (!response.IsSuccessStatusCode)
            {
                await ThrowUpstreamExceptionAsync(response, "fetching sellable products");
            }

            var products = await response.Content.ReadFromJsonAsync<List<SellableProductContractDto>>(JsonOptions)
                           ?? new List<SellableProductContractDto>();

            var normalizedPage = page < 1 ? 1 : page;
            var normalizedPageSize = pageSize < 1 ? 50 : pageSize;

            return products
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();
        });
    }

    public async Task<Dictionary<string, ProductLookupContractDto>> GetProductLookupMapAsync(string companyCen, IEnumerable<string> productCens)
    {
        return await ProductLookupPolicy.ExecuteAsync(async () =>
        {
            var cens = productCens
                .Where(cen => !string.IsNullOrWhiteSpace(cen))
                .Select(cen => cen.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cens.Count == 0)
            {
                return new Dictionary<string, ProductLookupContractDto>(StringComparer.OrdinalIgnoreCase);
            }

            var request = new ProductLookupContractRequest { ProductCens = cens };
            var path = BuildProductLookupPath(companyCen);

            Console.WriteLine($"Inventory lookup request (company={companyCen}) -> {cens.Count} cens: {string.Join(',', cens)}");

            using var response = await _httpClient.PostAsJsonAsync(path, request, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                await ThrowUpstreamExceptionAsync(response, "looking up products");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Inventory lookup response body: {responseBody}");

            var products = System.Text.Json.JsonSerializer.Deserialize<List<ProductLookupContractDto>>(responseBody, JsonOptions)
                           ?? new List<ProductLookupContractDto>();

            return products
                .Where(product => !string.IsNullOrWhiteSpace(product.ProductCen))
                .GroupBy(product => product.ProductCen, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        });
    }

    private static string BuildSellableProductsPath(
        string companyCen,
        string? search,
        string? categoryCen,
        string? warehouseCen,
        bool onlyAvailable)
    {
        var path = $"api/inventory/companies/{Uri.EscapeDataString(companyCen)}/sellable-products";
        var queryParameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            queryParameters.Add($"search={Uri.EscapeDataString(search)}");
        }

        if (!string.IsNullOrWhiteSpace(categoryCen))
        {
            queryParameters.Add($"categoryCen={Uri.EscapeDataString(categoryCen)}");
        }

        if (!string.IsNullOrWhiteSpace(warehouseCen))
        {
            queryParameters.Add($"warehouseCen={Uri.EscapeDataString(warehouseCen)}");
        }

        queryParameters.Add($"onlyAvailable={onlyAvailable.ToString().ToLowerInvariant()}");

        return $"{path}?{string.Join("&", queryParameters)}";
    }

    private static string BuildProductLookupPath(string companyCen)
        => $"api/inventory/companies/{Uri.EscapeDataString(companyCen)}/products/lookup";

    private async Task<string?> ResolveWarehouseCenAsync(string companyCen, string? warehouseCen)
    {
        if (!string.IsNullOrWhiteSpace(warehouseCen))
            return warehouseCen.Trim();

        return await SalesCenResolver.ResolveMainWarehouseCenAsync(_uow, companyCen);
    }

    private static async Task ThrowUpstreamExceptionAsync(HttpResponseMessage response, string operation)
    {
        var errorBody = await response.Content.ReadAsStringAsync();
        var message = ExtractUpstreamMessage(errorBody, response.StatusCode, operation);

        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
            {
                Dictionary<string, string[]>? errors = null;
                try
                {
                    using var doc = JsonDocument.Parse(errorBody);
                    if (doc.RootElement.TryGetProperty("errors", out var errorsEl) && errorsEl.ValueKind == JsonValueKind.Object)
                    {
                        errors = new Dictionary<string, string[]>();
                        foreach (var prop in errorsEl.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Array)
                            {
                                var list = prop.Value.EnumerateArray().Select(e => e.ToString()).ToArray();
                                errors[prop.Name] = list;
                            }
                            else
                            {
                                errors[prop.Name] = new[] { prop.Value.ToString() };
                            }
                        }
                    }
                }
                catch { }

                throw new ValidationException(message, errors);
            }
            case HttpStatusCode.NotFound:
                throw new NotFoundException(message);
            case HttpStatusCode.Conflict:
                throw new ConflictException(message);
            default:
                throw new Exception(message);
        }
    }

    public async Task ConsumeStockAsync(string companyCen, ConsumeStockContractRequest request)
    {
        await ConsumeStockPolicy.ExecuteAsync(async () =>
        {
            var path = $"api/inventory/companies/{Uri.EscapeDataString(companyCen)}/stock/consume";
            using var response = await _httpClient.PostAsJsonAsync(path, request, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                await ThrowUpstreamExceptionAsync(response, "consuming stock");
            }
        });
    }

    private static string ExtractUpstreamMessage(string errorBody, HttpStatusCode statusCode, string operation)
    {
        if (string.IsNullOrWhiteSpace(errorBody))
        {
            return $"Inventory API returned {(int)statusCode} ({statusCode}) while {operation}.";
        }

        try
        {
            using var document = JsonDocument.Parse(errorBody);
            if (document.RootElement.TryGetProperty("message", out var messageElement) &&
                messageElement.ValueKind == JsonValueKind.String)
            {
                return messageElement.GetString() ?? errorBody;
            }
        }
        catch
        {
        }

        return errorBody;
    }
}