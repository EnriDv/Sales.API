using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Shared.Core.Exceptions;

namespace Sales.API.Application.Services;

public class SalesCatalogService : ISalesCatalogService
{
    private readonly IUnitOfWork _uow;

    public SalesCatalogService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<SellableProductContractDto>> GetSellableProductsAsync(
        string companyCen,
        string? search = null,
        string? categoryCen = null,
        string? warehouseCen = null,
        bool onlyAvailable = true,
        int page = 1,
        int pageSize = 50)
    {
        if (!int.TryParse(companyCen, out var companyId))
            throw new ValidationException($"CEN de empresa inválido: {companyCen}");

        var company = await _uow.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new NotFoundException($"Empresa no encontrada: {companyCen}");

        var products = await _uow.Products.GetAllAsync(
            p => p.CompanyId == companyId && p.Active
                 && (search == null || p.Name.Contains(search) || p.Sku.Contains(search))
                 && (categoryCen == null || p.Category!.Code == categoryCen),
            "Category,Stocks.Warehouse");

        var query = products.AsEnumerable();

        if (onlyAvailable)
            query = query.Where(p => !p.IsOutOfStock);

        if (warehouseCen != null)
        {
            query = query.Where(p =>
                p.Stocks.Any(s => s.Warehouse.Code == warehouseCen && s.Quantity > 0));
        }

        return query
            .Skip(Math.Max(0, page - 1) * pageSize)
            .Take(pageSize)
            .Select(p =>
            {
                var stocks = warehouseCen == null
                    ? p.Stocks
                    : p.Stocks.Where(s => s.Warehouse.Code == warehouseCen);
                var available = stocks.Sum(s => s.Quantity);

                return new SellableProductContractDto
                {
                    ProductCen = p.Code,
                    Name = p.Name,
                    CategoryCen = p.Category?.Code ?? string.Empty,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    SalePrice = p.Price,
                    AvailableQuantity = available,
                    IsAvailable = !p.IsOutOfStock && available > 0,
                    StationCode = p.StationCode
                };
            })
            .ToList();
    }
}
