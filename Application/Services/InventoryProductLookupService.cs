using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Shared.Core.Cen;
using Shared.Core.Exceptions;

namespace Sales.API.Application.Services;

public class InventoryApiClient : IInventoryApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _uow;

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
    }

    public async Task<Dictionary<string, ProductLookupContractDto>> GetProductLookupMapAsync(string companyCen, IEnumerable<string> productCens)
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

        using var response = await _httpClient.PostAsJsonAsync(path, request, JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowUpstreamExceptionAsync(response, "looking up products");
        }

        var products = await response.Content.ReadFromJsonAsync<List<ProductLookupContractDto>>(JsonOptions)
                       ?? new List<ProductLookupContractDto>();

        return products
            .Where(product => !string.IsNullOrWhiteSpace(product.ProductCen))
            .GroupBy(product => product.ProductCen, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
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
        var path = $"api/inventory/companies/{Uri.EscapeDataString(companyCen)}/stock/consume";
        using var response = await _httpClient.PostAsJsonAsync(path, request, JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowUpstreamExceptionAsync(response, "consuming stock");
        }
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