using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Sales.API.Domain.Entities;
using Shared.Core.Cen;
using Shared.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Sales.API.Application.Services;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _uow;
    private readonly IInventoryApiClient _inventoryApiClient;
    private readonly Sales.API.Infrastructure.Persistence.SalesDbContext _ctx;

    public TicketService(IUnitOfWork uow, IInventoryApiClient inventoryApiClient, Sales.API.Infrastructure.Persistence.SalesDbContext ctx)
    {
        _uow = uow;
        _inventoryApiClient = inventoryApiClient;
        _ctx = ctx;
    }

    private async Task<int> GetOrderStatusIdAsync(string statusName)
    {
        var statuses = await _uow.OrderStatuses.GetAllAsync(s => s.Name.ToUpper() == statusName.ToUpper());
        var status = statuses.FirstOrDefault();
        if (status != null) return status.Id;
        
        return statusName.ToUpper() switch {
            "OPEN" => 1,
            "PAID" => 2,
            "CANCELLED" => 3,
            _ => 1
        };
    }

    private async Task<int> GetItemStatusIdAsync(string statusName)
    {
        var statuses = await _uow.RestaurantOrderDetailStatuses.GetAllAsync(s => s.Name.ToUpper() == statusName.ToUpper());
        var status = statuses.FirstOrDefault();
        if (status != null) return status.Id;
        
        return statusName.ToUpper() switch {
            "PENDING" => 1,
            "SENT" => 2,
            "IN_COMMAND" => 2,
            "COMPLETED" => 3,
            "PREPARING" => 2,
            "SERVED" => 3,
            "CANCELLED" => 3,
            _ => 1
        };
    }

    private async Task<decimal> GetCompanyTaxRateAsync(int companyId)
    {
        var taxConfig = (await _uow.TaxConfigurations.GetAllAsync(t => t.CompanyId == companyId)).FirstOrDefault();
        return taxConfig?.GlobalTaxPercentage ?? 0m;
    }

    // ── LIST ─────────────────────────────────────
    public async Task<List<TicketContractResponse>> GetTicketsAsync(string companyCen, string? status = null)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var query = await _uow.RestaurantOrders.GetAllAsync(
            t => t.Order.CompanyId == companyId && !t.IsDeleted && !t.Order.IsDeleted,
            "Order,RestaurantOrderDetails,RestaurantOrderDetails.RestaurantOrderDetailStatus,Order.OrderDetails,Order.Customer,Waiter,Order.OrderStatus"
        );
        
        if (!string.IsNullOrEmpty(status))
        {
            var normalizedStatus = status.Trim().ToUpperInvariant();
            if (normalizedStatus.Contains("CANCEL"))
            {
                query = query.Where(t => t.IsDeleted || t.Order.IsDeleted);
            }
            else
            {
                query = query.Where(t => !t.Order.IsDeleted && t.Order.OrderStatus?.Name?.ToUpper() == normalizedStatus);
            }
        }

        var results = new List<TicketContractResponse>();
        var taxRate = await GetCompanyTaxRateAsync(companyId);
        foreach (var t in query.OrderByDescending(x => x.CreatedAt))
            results.Add(await MapToContractResponseAsync(t, taxRate));
            
        return results;
    }

    // ── SINGLE ────────────────────────────────────
    public async Task<TicketContractResponse> GetTicketAsync(string companyCen, string ticketCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);
        var taxRate = await GetCompanyTaxRateAsync(companyId);
        return await MapToContractResponseAsync(ticket, taxRate);
    }

    public async Task<TicketTotalsContractResponse> GetTicketTotalsAsync(string companyCen, string ticketCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);
        var taxRate = await GetCompanyTaxRateAsync(companyId);
        var amounts = CalculateTicketAmounts(ticket, taxRate);

        return new TicketTotalsContractResponse
        {
            TicketCen = CenParser.Format(ticket.Cen),
            Subtotal = amounts.Subtotal,
            TaxAmount = amounts.TaxAmount,
            Total = amounts.TotalAmount
        };
    }

    // ── CREATE ────────────────────────────────────
    public async Task<TicketContractResponse> CreateTicketAsync(string companyCen, CreateTicketContractRequest request)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var taxRate = await GetCompanyTaxRateAsync(companyId);

        // Resolve waiter by CEN
        int waiterId = 1; // Default
        if (!string.IsNullOrWhiteSpace(request.WaiterCen) && CenParser.TryParse(request.WaiterCen, out var waiterCen))
        {
            var waiter = (await _uow.Waiters.GetAllAsync(w => w.Cen == waiterCen)).FirstOrDefault();
            if (waiter != null) waiterId = waiter.Id;
        }

        int openStatusId = await GetOrderStatusIdAsync("OPEN");
        var companyCenGuid = CenParser.ParseRequired(companyCen, "empresa");

        var order = new Order
        {
            CompanyId = companyId,
            CompanyCen = companyCenGuid,
            CustomerId = 1, // Default customer
            OrderStatusId = openStatusId,
            TaxPrice = 0,
            OrderDatetime = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Assign a DailyNumber (max for company today + 1)
        var today = DateTime.UtcNow.Date;
        var todaysOrders = await _uow.Orders.GetAllAsync(o => o.CompanyId == companyId && o.CreatedAt >= today && o.CreatedAt < today.AddDays(1));
        var maxDaily = todaysOrders.Any() ? todaysOrders.Max(o => o.DailyNumber) : 0;
        order.DailyNumber = maxDaily + 1;

        await _uow.Orders.AddAsync(order);
        await _uow.SaveAsync(); // Save to get Order.Id

        var restaurantOrder = new RestaurantOrder
        {
            Cen = Guid.NewGuid(),
            OrderId = order.Id,
            WaiterId = waiterId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _uow.RestaurantOrders.AddAsync(restaurantOrder);
        await _uow.SaveAsync();

        return await GetTicketAsync(companyCen, CenParser.Format(restaurantOrder.Cen));
    }

    // ── ASSIGN WAITER ─────────────────────────────
    public async Task<AssignTicketWaiterContractResponse> AssignWaiterAsync(string companyCen, string ticketCen, AssignTicketWaiterContractRequest request)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);
        
        var waiterCenGuid = CenParser.ParseRequired(request.WaiterCen, "mesero");
        var waiter = (await _uow.Waiters.GetAllAsync(w => w.Cen == waiterCenGuid)).FirstOrDefault()
            ?? throw new NotFoundException($"Mesero no encontrado: {request.WaiterCen}");

        ticket.WaiterId = waiter.Id;
        _uow.RestaurantOrders.Update(ticket);
        await _uow.SaveAsync();

        return new AssignTicketWaiterContractResponse
        {
            TicketCen = CenParser.Format(ticket.Cen),
            WaiterCen = CenParser.Format(waiter.Cen),
            WaiterName = waiter.Name
        };
    }

    // ── SEND TO KITCHEN ────────────────────────────
    public async Task SendToKitchenAsync(string companyCen, string ticketCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);

        var pendingId = await GetItemStatusIdAsync("PENDING");
        var sentId = await GetItemStatusIdAsync("SENT");

        foreach (var item in ticket.RestaurantOrderDetails.Where(i => i.RestaurantOrderDetailStatusId == pendingId && !i.IsDeleted))
        {
            item.RestaurantOrderDetailStatusId = sentId;
            item.SentAt = DateTime.UtcNow;
            _uow.RestaurantOrderDetails.Update(item);
        }
        await _uow.SaveAsync();
    }

    public async Task<PayTicketContractResponse> PayTicketAsync(string companyCen, string ticketCen, PayTicketContractRequest request)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);
        var taxRate = await GetCompanyTaxRateAsync(companyId);

        var amounts = CalculateTicketAmounts(ticket, taxRate);

        var paymentType = (await _uow.PaymentTypes.GetAllAsync(p => p.Name.ToUpper() == request.PaymentMethodCode.ToUpper())).FirstOrDefault();
        var paymentTypeId = paymentType?.Id ?? 1;

        var companyCenGuid = CenParser.ParseRequired(companyCen, "empresa");

        await using var tx = await _ctx.Database.BeginTransactionAsync();
        try
        {
            var sale = new Sale
            {
                Cen = Guid.NewGuid(),
                CompanyId = companyId,
                CompanyCen = companyCenGuid,
                CustomerId = ticket.Order.CustomerId,
                PaymentTypeId = paymentTypeId,
                SubtotalPrice = amounts.Subtotal,
                TaxPrice = amounts.TaxAmount,
                SaleDatetime = DateTime.UtcNow,
                DiscountPercentage = 0,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _uow.Sales.AddAsync(sale);

            ticket.Order.OrderStatusId = await GetOrderStatusIdAsync("PAID");
            _uow.Orders.Update(ticket.Order);

            await _uow.SaveAsync();

            var mainWarehouse = await SalesCenResolver.ResolveMainWarehouseCenAsync(_uow, companyCen);

            var consumeRequest = new ConsumeStockContractRequest
            {
                WarehouseCen = mainWarehouse,
                Source = "SALES",
                ReferenceCen = CenParser.Format(sale.Cen),
                Reason = $"Pago ticket {CenParser.Format(ticket.Cen)}",
                Items = ticket.RestaurantOrderDetails
                    .Where(i => !i.IsDeleted)
                    .Select(i => new StockItemRequest
                    {
                        ProductCen = CenParser.Format(i.ProductCen),
                        Quantity = i.Quantity
                    })
                    .ToList()
            };
            await _inventoryApiClient.ConsumeStockAsync(CenParser.Format(ticket.Order.CompanyCen), consumeRequest);
            await tx.CommitAsync();
            return new PayTicketContractResponse
            {
                SaleCen = CenParser.Format(sale.Cen),
                TicketCen = CenParser.Format(ticket.Cen),
                Status = "PAID",
                Subtotal = amounts.Subtotal,
                TaxAmount = amounts.TaxAmount,
                Total = amounts.TotalAmount
            };
        }
        catch (Shared.Core.Exceptions.ConflictException)
        {
            await tx.RollbackAsync();
            throw;
        }
        catch (Shared.Core.Exceptions.ValidationException)
        {
            await tx.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            throw new Exception($"Error procesando pago: {ex.Message}", ex);
        }
    }

    public async Task<CancelTicketContractResponse> CancelTicketAsync(string companyCen, string ticketCen, CancelTicketContractRequest? request = null)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);

        ticket.IsDeleted = true;
        ticket.Order.IsDeleted = true;
        _uow.Orders.Update(ticket.Order);
        _uow.RestaurantOrders.Update(ticket);

        foreach (var item in ticket.RestaurantOrderDetails)
        {
            item.IsDeleted = true;
            _uow.RestaurantOrderDetails.Update(item);
        }

        await _uow.SaveAsync();

        return new CancelTicketContractResponse
        {
            TicketCen = CenParser.Format(ticket.Cen),
            Status = "CANCELLED"
        };
    }

    public async Task<FileContentResult> PrintTicketAsync(string companyCen, string ticketCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);
        var taxRate = await GetCompanyTaxRateAsync(companyId);
        var ticketData = await MapToContractResponseAsync(ticket, taxRate);

        var sb = new StringBuilder();
        sb.AppendLine("TICKET DE VENTA");
        sb.AppendLine($"Ticket: {ticketData.TicketCen}");
        sb.AppendLine($"Empresa: {ticketData.CompanyCen}");
        sb.AppendLine($"Fecha: {ticketData.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Mesero: {ticketData.WaiterCen ?? "N/A"}");
        sb.AppendLine($"Estado: {ticketData.Status}");
        sb.AppendLine(new string('-', 40));

        foreach (var item in ticketData.Items)
        {
            sb.AppendLine($"{item.Quantity} x {item.ProductName} @ {item.UnitPrice:F2}");
            if (!string.IsNullOrWhiteSpace(item.Note))
                sb.AppendLine($"  Nota: {item.Note}");
        }

        sb.AppendLine(new string('-', 40));
        sb.AppendLine($"Subtotal: {ticketData.Subtotal:F2}");
        sb.AppendLine($"Impuesto: {ticketData.TaxAmount:F2}");
        sb.AppendLine($"Total: {ticketData.TotalAmount:F2}");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"ticket-{ticketData.DailyNumber}-{ticketData.TicketCen}.txt";

        return new FileContentResult(bytes, "text/plain")
        {
            FileDownloadName = fileName
        };
    }

    public async Task<List<TicketItemContractResponse>> GetTicketItemsAsync(string companyCen, string ticketCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);
        var lookups = await GetProductLookupMapAsync(companyCen, ticket.RestaurantOrderDetails.Select(i => CenParser.Format(i.ProductCen)));
        return ticket.RestaurantOrderDetails.Select(i => MapItemToContract(i, lookups)).ToList();
    }

    public async Task<TicketContractResponse> AddItemAsync(string companyCen, string ticketCen, CreateTicketItemContractRequest request)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);

        var productCenGuid = CenParser.ParseRequired(request.ProductCen, "producto");
        var effectiveCompanyCen = CenParser.Format(ticket.Order.CompanyCen);
        var productLookup = await GetProductLookupAsync(effectiveCompanyCen, request.ProductCen);
        decimal unitPrice = productLookup.SalePrice;

        var pendingId = await GetItemStatusIdAsync("PENDING");

        var item = new RestaurantOrderDetail
        {
            Cen = Guid.NewGuid(),
            RestaurantOrderId = ticket.Id,
            ProductId = 1, 
            ProductCen = productCenGuid,
            Quantity = request.Quantity,
            RestaurantOrderDetailStatusId = pendingId,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _uow.RestaurantOrderDetails.AddAsync(item);
        
        var orderDetail = new OrderDetail {
            OrderId = ticket.Order.Id,
            ProductId = 1,
            ProductCen = productCenGuid,
            Quantity = request.Quantity,
            ProductPrice = unitPrice,
            CreatedAt = DateTime.UtcNow
        };
        await _uow.OrderDetails.AddAsync(orderDetail);
        
        await _uow.SaveAsync();

        return await GetTicketAsync(companyCen, ticketCen);
    }

    public async Task<TicketContractResponse> UpdateItemAsync(string companyCen, string ticketCen, string ticketItemCen, UpdateTicketItemContractRequest request)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);

        var itemCen = CenParser.ParseRequired(ticketItemCen, "item de ticket");
        var item = ticket.RestaurantOrderDetails.FirstOrDefault(i => i.Cen == itemCen)
            ?? throw new NotFoundException($"Item no encontrado: {ticketItemCen}");

        if (request.Quantity.HasValue) item.Quantity = request.Quantity.Value;
        if (request.Note != null) item.Note = request.Note;

        _uow.RestaurantOrderDetails.Update(item);
        await _uow.SaveAsync();

        return await GetTicketAsync(companyCen, ticketCen);
    }

    public async Task ResendItemAsync(string companyCen, string ticketCen, string ticketItemCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);

        var itemCen = CenParser.ParseRequired(ticketItemCen, "item de ticket");
        var item = ticket.RestaurantOrderDetails.FirstOrDefault(i => i.Cen == itemCen)
            ?? throw new NotFoundException($"Item no encontrado: {ticketItemCen}");

        item.IsDeleted = false;
        item.RestaurantOrderDetailStatusId = await GetItemStatusIdAsync("PENDING");
        item.ResendCount++;
        _uow.RestaurantOrderDetails.Update(item);
        await _uow.SaveAsync();
    }


    private static (decimal Subtotal, decimal TaxAmount, decimal TotalAmount) CalculateTicketAmounts(
        RestaurantOrder ticket, decimal taxRate)
    {
        decimal subtotal = ticket.Order.OrderDetails.Sum(od => od.ProductPrice * od.Quantity);
        decimal taxAmount = subtotal * (taxRate / 100m);
        return (subtotal, taxAmount, subtotal + taxAmount);
    }

    private async Task<TicketContractResponse> MapToContractResponseAsync(RestaurantOrder t, decimal taxRate)
    {
        var amounts = CalculateTicketAmounts(t, taxRate);
        var lookups = await GetProductLookupMapAsync(CenParser.Format(t.Order.CompanyCen), t.RestaurantOrderDetails.Select(i => CenParser.Format(i.ProductCen)));
        return new TicketContractResponse
        {
            TicketCen = CenParser.Format(t.Cen),
            DailyNumber = t.Order.DailyNumber,
            Status = MapTicketStatus(t),
            CreatedAt = t.CreatedAt,
            WaiterCen = t.Waiter != null ? CenParser.Format(t.Waiter.Cen) : null,
            CompanyCen = CenParser.Format(t.Order.CompanyCen),
            TaxAmount = amounts.TaxAmount,
            Subtotal = amounts.Subtotal,
            TotalAmount = amounts.TotalAmount,
            Items = t.RestaurantOrderDetails.Select(i => MapItemToContract(i, lookups)).ToList()
        };
    }

    private async Task<ProductLookupContractDto> GetProductLookupAsync(string companyCen, string productCen)
    {
        var normalizedProductCen = CenParser.Format(CenParser.ParseRequired(productCen, "producto"));
        var lookups = await GetProductLookupMapAsync(companyCen, new[] { normalizedProductCen });

        return lookups.TryGetValue(normalizedProductCen, out var product)
            ? product
            : lookups.Values.FirstOrDefault()
            ?? throw new NotFoundException($"Producto no encontrado en inventario: {normalizedProductCen}");
    }

    private Task<Dictionary<string, ProductLookupContractDto>> GetProductLookupMapAsync(string companyCen, IEnumerable<string> productCens)
        => _inventoryApiClient.GetProductLookupMapAsync(companyCen, productCens);

    private static TicketItemContractResponse MapItemToContract(RestaurantOrderDetail i, IReadOnlyDictionary<string, ProductLookupContractDto> lookups) => new()
    {
        TicketItemCen = CenParser.Format(i.Cen),
        ProductCen = CenParser.Format(i.ProductCen),
        ProductName = lookups.TryGetValue(CenParser.Format(i.ProductCen), out var product)
            ? product.Name
            : $"Producto no encontrado ({CenParser.Format(i.ProductCen)})",
        Quantity = i.Quantity,
        UnitPrice = lookups.TryGetValue(CenParser.Format(i.ProductCen), out var matchedProduct)
            ? matchedProduct.SalePrice
            : 0,
        Status = MapTicketItemStatus(i),
        Note = i.Note,
        SentAt = i.SentAt,
        ResendCount = i.ResendCount
    };

    private static string MapTicketStatus(RestaurantOrder ticket)
    {
        if (ticket.IsDeleted || ticket.Order.IsDeleted)
        {
            return "CANCELLED";
        }

        return ticket.Order.OrderStatus?.Name?.ToUpperInvariant() ?? "OPEN";
    }

    private static string MapTicketItemStatus(RestaurantOrderDetail item)
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