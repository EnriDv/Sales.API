using System.Net;
using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;

namespace Sales.API.Application.Services;

public class SalesCatalogService : ISalesCatalogService
{
    private readonly IInventoryApiClient _inventoryApiClient;

    public SalesCatalogService(IInventoryApiClient inventoryApiClient)
    {
        _inventoryApiClient = inventoryApiClient;
    }

    public Task<List<SellableProductContractDto>> GetSellableProductsAsync(
        string companyCen,
        string? search = null,
        string? categoryCen = null,
        string? warehouseCen = null,
        bool onlyAvailable = true,
        int page = 1,
        int pageSize = 50)
    {
        return _inventoryApiClient.GetSellableProductsAsync(companyCen, search, categoryCen, warehouseCen, onlyAvailable, page, pageSize);
    }
}
