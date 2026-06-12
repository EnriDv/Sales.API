using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Sales.API.Shared.Cen;
using Microsoft.EntityFrameworkCore;

namespace Sales.API.Application.Services;

public class SalesDashboardService : ISalesDashboardService
{
    private readonly IUnitOfWork _uow;
    private readonly IInventoryApiClient _inventoryApiClient;

    public SalesDashboardService(IUnitOfWork uow, IInventoryApiClient inventoryApiClient)
    {
        _uow = uow;
        _inventoryApiClient = inventoryApiClient;
    }

    public async Task<DailySalesDashboardDto> GetDailySalesAsync(string companyCen, DateTime? date = null)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var targetDate = date?.Date ?? DateTime.UtcNow.Date;

        var sales = await _uow.Sales.GetAllAsync(
            s => s.CompanyId == companyId && s.CreatedAt >= targetDate && s.CreatedAt < targetDate.AddDays(1)
        );

        return new DailySalesDashboardDto
        {
            TotalSales = sales.Sum(s => s.SubtotalPrice + s.TaxPrice),
            TicketsCount = sales.Count(),
            AverageTicket = sales.Any() ? sales.Average(s => s.SubtotalPrice + s.TaxPrice) : 0
        };
    }

    public async Task<List<TopProductDashboardContractResponse>> GetTopProductsAsync(string companyCen, int topN = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var sales = await _uow.Sales.GetAllAsync(
            s => s.CompanyId == companyId
                 && !s.IsDeleted
                 && (startDate == null || s.SaleDatetime >= startDate.Value)
                 && (endDate == null || s.SaleDatetime < endDate.Value),
            "SaleDetails");
        var productCens = sales
            .SelectMany(s => s.SaleDetails)
            .Select(i => CenParser.Format(i.ProductCen))
            .Where(cen => !string.IsNullOrWhiteSpace(cen))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        Console.WriteLine($"Found {productCens.Count} unique product cens in sales data for company {companyCen} between {startDate} and {endDate}.");

        var productLookupMap = await _inventoryApiClient.GetProductLookupMapAsync(companyCen, productCens);

        Console.WriteLine($"Retrieved product lookup map with {productLookupMap.Count} entries for company {companyCen}.");
        
        var topProducts = sales
            .SelectMany(s => s.SaleDetails)
            .GroupBy(i => i.ProductCen)
            .Select(g => new TopProductDashboardContractResponse
            {
                ProductCen = CenParser.Format(g.Key),
                ProductName = productLookupMap.TryGetValue(CenParser.Format(g.Key), out var product)
                    ? product.Name
                    : $"Producto no encontrado ({CenParser.Format(g.Key)})",
                TotalQuantity = g.Sum(i => i.Quantity),
                CategoryCen = productLookupMap.TryGetValue(CenParser.Format(g.Key), out var matchedProductCategory)
                    ? matchedProductCategory.CategoryCen
                    : null,
                CategoryName = productLookupMap.TryGetValue(CenParser.Format(g.Key), out var matchedProductCategoryName)
                    ? matchedProductCategoryName.CategoryName
                    : null,
                SalePrice = productLookupMap.TryGetValue(CenParser.Format(g.Key), out var matchedProduct)
                    ? matchedProduct.SalePrice
                    : g.First().Price
            })
            .OrderByDescending(p => p.TotalQuantity)
            .Take(topN)
            .ToList();

            Console.WriteLine($"Calculated top {topN} products for company {companyCen} between {startDate} and {endDate}.");

        return topProducts;
    }

    public async Task<KdsStatusDashboardDto> GetKdsStatusAsync(string companyCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        // Only active items from RestaurantOrderDetails
        var items = await _uow.RestaurantOrderDetails.GetAllAsync(
            i => i.RestaurantOrder.Order.CompanyId == companyId && i.RestaurantOrder.Order.OrderStatusId != 4, // 4 = closed/paid?
            "RestaurantOrderDetailStatus,RestaurantOrder.Order");

        return new KdsStatusDashboardDto
        {
            PendingCount = items.Count(i => i.RestaurantOrderDetailStatusId == 1), // Assuming 1 = Pending
            PreparingCount = items.Count(i => i.RestaurantOrderDetailStatusId == 2), // Assuming 2 = Preparing
            ReadyCount = items.Count(i => i.RestaurantOrderDetailStatusId == 3) // Assuming 3 = Ready
        };
    }
}
