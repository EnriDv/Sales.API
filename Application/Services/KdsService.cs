using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Sales.API.Domain.Entities;
using Shared.Core.Cen;
using Shared.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Sales.API.Application.Services;

public class KdsService : IKdsService
{
    private readonly IUnitOfWork _uow;
    private readonly IInventoryApiClient _inventoryApiClient;

    public KdsService(IUnitOfWork uow, IInventoryApiClient inventoryApiClient)
    {
        _uow = uow;
        _inventoryApiClient = inventoryApiClient;
    }

    public async Task<List<KdsTeamContractResponse>> GetTeamsAsync(string companyCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var teams = await _uow.Teams.GetAllAsync();
        return teams.Select(t => new KdsTeamContractResponse
        {
            TeamCen = CenParser.Format(t.Cen),
            Name = t.Name,
            CategoryCens = new List<string>()
        }).ToList();
    }

    public async Task<List<KdsItemContractResponse>> GetTeamItemsAsync(string companyCen, string teamCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        // Returns all active items that are not pending/cancelled 
        // Real implementation would filter by Team configurations
        var items = await _uow.RestaurantOrderDetails.GetAllAsync(
            i => i.RestaurantOrder.Order.CompanyId == companyId && !i.IsDeleted,
            "RestaurantOrder.Order,RestaurantOrderDetailStatus");

        var lookups = await _inventoryApiClient.GetProductLookupMapAsync(companyCen, items.Select(i => CenParser.Format(i.ProductCen)));

        return items.Select(i => new KdsItemContractResponse
        {
            TicketItemCen = CenParser.Format(i.Cen),
            TicketCen = CenParser.Format(i.RestaurantOrder.Cen),
            ProductCen = CenParser.Format(i.ProductCen),
            ProductName = lookups.TryGetValue(CenParser.Format(i.ProductCen), out var product)
                ? product.Name
                : $"Producto no encontrado ({CenParser.Format(i.ProductCen)})",
            Quantity = i.Quantity,
            Status = MapKdsStatus(i),
            Note = i.Note,
            ResendCount = i.ResendCount,
            CreatedAt = i.CreatedAt
        }).ToList();
    }

    public async Task UpdateItemStatusAsync(string companyCen, string ticketItemCen, UpdateKdsItemStatusContractRequest request)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var itemCen = CenParser.ParseRequired(ticketItemCen, "ticket item");

        var items = await _uow.RestaurantOrderDetails.GetAllAsync(i => i.Cen == itemCen && i.RestaurantOrder.Order.CompanyId == companyId);
        var item = items.FirstOrDefault() ?? throw new NotFoundException($"Item no encontrado: {ticketItemCen}");

        var normalizedStatus = request.Status.Trim().ToUpperInvariant();
        if (normalizedStatus.Contains("CANCEL"))
        {
            item.IsDeleted = true;
        }
        else
        {
            var targetStatusName = normalizedStatus switch
            {
                "DELIVERED" => "COMPLETED",
                "READY" => "COMPLETED",
                "PREPARING" => "SENT",
                "CREATED" => "PENDING",
                _ => normalizedStatus
            };

            var status = (await _uow.RestaurantOrderDetailStatuses.GetAllAsync(s => s.Name.ToUpper() == targetStatusName)).FirstOrDefault();
            if (status != null)
            {
                item.RestaurantOrderDetailStatusId = status.Id;
            }
        }

        _uow.RestaurantOrderDetails.Update(item);
        await _uow.SaveAsync();
    }

    private static string MapKdsStatus(RestaurantOrderDetail item)
    {
        if (item.IsDeleted || item.RestaurantOrder.IsDeleted || item.RestaurantOrder.Order.IsDeleted)
        {
            return "canceled";
        }

        var statusName = item.RestaurantOrderDetailStatus?.Name?.Trim().ToUpperInvariant() ?? "PENDING";
        return statusName switch
        {
            "PENDING" => "created",
            "SENT" => "preparing",
            "IN_COMMAND" => "preparing",
            "COMPLETED" => "delivered",
            "SERVED" => "delivered",
            "CANCELLED" => "canceled",
            _ => "created"
        };
    }
}
