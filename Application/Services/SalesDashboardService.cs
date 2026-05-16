using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Shared.Core.Exceptions;

namespace Sales.API.Application.Services;

public class SalesDashboardService : ISalesDashboardService
{
    private readonly IUnitOfWork _uow;

    public SalesDashboardService(IUnitOfWork uow) => _uow = uow;

    private async Task<int> ResolveCompanyIdAsync(string companyCen)
    {
        if (!int.TryParse(companyCen, out var id))
            throw new ValidationException($"CEN de empresa inválido: {companyCen}");
        var company = await _uow.Companies.GetByIdAsync(id);
        if (company == null)
            throw new NotFoundException($"Empresa no encontrada: {companyCen}");
        return id;
    }

    public async Task<DailySalesDashboardContractDto> GetDailySalesAsync(string companyCen, DateTime? date = null)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var targetDate = date?.Date ?? DateTime.UtcNow.Date;

        var tickets = await _uow.Tickets.GetAllAsync(
            t => t.CompanyId == companyId && t.CreatedAt >= targetDate && t.CreatedAt < targetDate.AddDays(1)
        );

        var paidTickets = tickets.Where(t => t.Status == "PAID").ToList();

        return new DailySalesDashboardContractDto
        {
            Date = targetDate,
            TotalSales = paidTickets.Sum(t => t.TotalAmount),
            TicketsCount = tickets.Count(),
            AverageTicket = paidTickets.Any() ? paidTickets.Average(t => t.TotalAmount) : 0
        };
    }

    public async Task<List<TopProductDashboardContractDto>> GetTopProductsAsync(string companyCen, int topN = 10)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var tickets = await _uow.Tickets.GetAllAsync(t => t.CompanyId == companyId && t.Status == "PAID", "Items.Product");
        
        var topProducts = tickets
            .SelectMany(t => t.Items)
            .Where(i => i.Status != "CANCELLED")
            .GroupBy(i => new { i.Product?.Code, i.Product?.Name })
            .Select(g => new TopProductDashboardContractDto
            {
                ProductCen = g.Key.Code ?? string.Empty,
                ProductName = g.Key.Name ?? string.Empty,
                TotalQuantity = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.Subtotal > 0 ? i.Subtotal : i.UnitPrice * i.Quantity),
                OrderCount = g.Count()
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(topN)
            .ToList();

        return topProducts;
    }

    public async Task<KdsDashboardStatusContractDto> GetKdsStatusAsync(string companyCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        // Only active tickets
        var tickets = await _uow.Tickets.GetAllAsync(t => t.CompanyId == companyId && t.Status == "OPEN", "Items");
        var items = tickets.SelectMany(t => t.Items).ToList();

        return new KdsDashboardStatusContractDto
        {
            Pending = items.Count(i => i.Status == "PENDING"),
            Preparing = items.Count(i => i.Status == "IN_COMMAND"),
            Delivered = items.Count(i => i.Status == "SERVED"),
            Total = items.Count(i => i.Status != "CANCELLED")
        };
    }
}
