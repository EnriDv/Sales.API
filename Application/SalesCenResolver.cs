using Sales.API.Application.Interfaces;
using Sales.API.Domain.Entities;
using Sales.API.Shared.Cen;
using Sales.API.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Sales.API.Application;

public static class SalesCenResolver
{
    public static async Task<Order> ResolveCompanyAndOrderAsync(IUnitOfWork uow, string companyCen, string orderCen)
    {
        var companyId = await ResolveCompanyIdAsync(uow, companyCen);
        var cen = CenParser.ParseRequired(orderCen, "order");
        var order = (await uow.Orders.GetAllAsync(o => o.CompanyId == companyId && o.RestaurantOrders.Any(ro => ro.Cen == cen), "RestaurantOrders,OrderDetails")).FirstOrDefault()
            ?? throw new NotFoundException($"Orden no encontrada: {orderCen}");
        return order;
    }

    public static async Task<RestaurantOrder> ResolveTicketAsync(IUnitOfWork uow, int companyId, string ticketCen)
    {
        var cen = CenParser.ParseRequired(ticketCen, "ticket");
        var ticket = (await uow.RestaurantOrders.GetAllAsync(
            t => t.Order.CompanyId == companyId && t.Cen == cen,
            "Order,RestaurantOrderDetails,RestaurantOrderDetails.RestaurantOrderDetailStatus,Order.OrderDetails,Order.Customer,Waiter"))
            .FirstOrDefault();
            
        return ticket ?? throw new NotFoundException($"Ticket no encontrado: {ticketCen}");
    }

    public static async Task<int> ResolveCompanyIdAsync(IUnitOfWork uow, string companyCen)
    {
        var cen = CenParser.ParseRequired(companyCen, "empresa");
        var taxConfig = (await uow.TaxConfigurations.GetAllAsync(t => t.CompanyCen == cen)).FirstOrDefault();
        if (taxConfig == null) throw new NotFoundException($"Empresa no configurada en ventas: {companyCen}");
        return taxConfig.CompanyId;
    }

    public static async Task<string> ResolveMainWarehouseCenAsync(IUnitOfWork uow, string companyCen)
    {
        var companyId = await ResolveCompanyIdAsync(uow, companyCen);
        var config = (await uow.WarehouseConfigurations.GetAllAsync(w => w.CompanyId == companyId && !w.IsDeleted)).FirstOrDefault();

        if (config == null)
            throw new NotFoundException($"Configuración de almacén no encontrada para la empresa: {companyCen}");

        return CenParser.Format(config.MainWarehouseCen);
    }
}
