using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Sales.API.Domain.Entities;
using Shared.Core.Exceptions;

namespace Sales.API.Application.Services;

public class TicketService : ITicketService
{
    private static readonly HashSet<string> AllowedTicketItemStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "PENDING",
        "IN_COMMAND",
        "SERVED",
        "CANCELLED",
    };

    private readonly IUnitOfWork _uow;

    public TicketService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<TicketResponse>> GetActiveTicketsAsync(int companyId, int locationId)
    {
        var tickets = await _uow.TicketQueries.GetActiveTicketsAsync(companyId, locationId);
        return tickets.Select(MapToResponse).ToList();
    }

    public async Task<List<TicketResponse>> GetTicketHistoryAsync(int companyId, int locationId, int take = 100)
    {
        var tickets = await _uow.TicketQueries.GetTicketHistoryAsync(companyId, locationId, take);
        return tickets.Select(MapToResponse).ToList();
    }

    public async Task<TicketResponse> GetTicketAsync(int companyId, int ticketId)
    {
        var ticket = await _uow.TicketQueries.GetTicketWithDetailsAsync(companyId, ticketId);
        if (ticket == null) throw new NotFoundException("Ticket no encontrado.");
        return MapToResponse(ticket);
    }

    public async Task<TicketResponse> CreateTicketAsync(int companyId, CreateTicketRequest request)
    {
        if (request.LocationId <= 0)
        {
            throw new DomainException("Error: El LocationId es 0 o inválido. Revisa los datos enviados.");
        }

        var settingsList = await _uow.SalesSettings.GetAllAsync(s => s.CompanyId == companyId);
        var settings = settingsList.FirstOrDefault();
        decimal defaultTaxRate = settings?.TaxRate ?? 0;

        var ticket = new Ticket
        {
            CompanyId = companyId,
            LocationId = request.LocationId,
            TicketNumber = BuildTicketNumber(),
            VendorId = request.VendorId,
            CustomerId = request.CustomerId,
            ServiceType = request.ServiceType,
            TableCode = request.TableCode,
            Status = "OPEN",
            Subtotal = 0,
            TaxRate = defaultTaxRate,
            TaxAmount = 0,
            TotalAmount = 0,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            OpenedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.Tickets.AddAsync(ticket);
        await _uow.SaveAsync();

        return await GetTicketAsync(companyId, ticket.Id);
    }

    public async Task<TicketResponse> AddItemAsync(int companyId, int ticketId, AddTicketItemRequest request)
    {
        if (request.ProductId <= 0)
        {
            throw new DomainException("Error: El ProductId es 0 o inválido.");
        }

        var ticket = await _uow.TicketQueries.GetTicketWithDetailsAsync(companyId, ticketId);
        if (ticket == null) throw new NotFoundException("Ticket no encontrado.");
        if (ticket.Status != "OPEN") throw new DomainException("El ticket ya está pagado o cancelado.");

        var product = await _uow.Products.GetByIdAsync(request.ProductId);
        if (product == null || product.CompanyId != companyId) throw new NotFoundException("Producto no encontrado.");

        decimal unitPrice = request.UnitPrice > 0 ? request.UnitPrice : product.Price;

        var item = new TicketItem
        {
            TicketId = ticketId,
            ProductId = request.ProductId,
            Quantity = request.Quantity > 0 ? request.Quantity : 1,
            UnitPrice = unitPrice,
            Status = "PENDING",
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.TicketItems.AddAsync(item);
        await _uow.SaveAsync();

        return await GetTicketAsync(companyId, ticketId);
    }

    public async Task<TicketResponse> UpdateItemStatusAsync(int companyId, int ticketId, int itemId, UpdateTicketItemStatusRequest request)
    {
        var ticket = await _uow.TicketQueries.GetTicketWithDetailsAsync(companyId, ticketId);
        if (ticket == null) throw new NotFoundException("Ticket no encontrado.");
        if (!string.Equals(ticket.Status, "OPEN", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Solo puedes actualizar items en tickets abiertos.");
        }

        var item = ticket.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) throw new NotFoundException("Item del ticket no encontrado.");

        item.Status = NormalizeItemStatus(request.Status);
        item.UpdatedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        _uow.TicketItems.Update(item);
        _uow.Tickets.Update(ticket);
        await _uow.SaveAsync();

        return await GetTicketAsync(companyId, ticketId);
    }

    public async Task<TicketResponse> CheckoutAsync(int companyId, int ticketId, CheckoutRequest request)
    {
        var ticket = await _uow.TicketQueries.GetTicketWithDetailsAsync(companyId, ticketId);
        if (ticket == null) throw new NotFoundException("Ticket no encontrado.");
        if (ticket.Status != "OPEN") throw new DomainException("El ticket no está abierto para cobro.");
        if (!ticket.Items.Any()) throw new DomainException("No se puede cobrar un ticket vacío.");

        var calculatedAmounts = CalculateTicketAmounts(ticket.Items, ticket.TaxRate);
        ticket.Subtotal = calculatedAmounts.Subtotal;
        ticket.TaxAmount = calculatedAmounts.TaxAmount;
        ticket.TotalAmount = calculatedAmounts.TotalAmount;

        decimal amountToPay = request.Amount > 0 ? request.Amount : ticket.TotalAmount;

        var payment = new Payment
        {
            TicketId = ticketId,
            PaymentMethod = request.PaymentMethod,
            Amount = amountToPay,
            Reference = request.Reference,
            PaidBy = request.PaidBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        ticket.Status = "PAID";
        ticket.PaidAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _uow.Payments.AddAsync(payment);
        _uow.Tickets.Update(ticket);
        await _uow.SaveAsync();

        return await GetTicketAsync(companyId, ticketId);
    }

    public async Task CancelTicketAsync(int companyId, int ticketId)
    {
        var ticket = await _uow.TicketQueries.GetTicketWithDetailsAsync(companyId, ticketId);
        if (ticket == null) throw new NotFoundException("Ticket no encontrado.");
        if (ticket.Status == "PAID") throw new DomainException("No se puede cancelar un ticket pagado.");

        ticket.Status = "CANCELLED";
        ticket.CancelledAt = DateTime.UtcNow;
        ticket.Subtotal = 0;
        ticket.TaxAmount = 0;
        ticket.TotalAmount = 0;
        ticket.UpdatedAt = DateTime.UtcNow;

        foreach (var item in ticket.Items)
        {
            item.Status = "CANCELLED";
            item.UpdatedAt = DateTime.UtcNow;
            _uow.TicketItems.Update(item);
        }

        _uow.Tickets.Update(ticket);
        await _uow.SaveAsync();
    }

    private static string BuildTicketNumber()
    {
        // Incluye milisegundos y sufijo aleatorio para evitar colisiones por concurrencia.
        return $"T-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
    }

    private static string NormalizeItemStatus(string? status)
    {
        var normalized = string.IsNullOrWhiteSpace(status) ? string.Empty : status.Trim().ToUpperInvariant();
        if (!AllowedTicketItemStatuses.Contains(normalized))
        {
            throw new DomainException("Estado de item de ticket inválido.");
        }

        return normalized;
    }

    private static TicketResponse MapToResponse(Ticket t)
    {
        var calculatedAmounts = CalculateTicketAmounts(t.Items, t.TaxRate);

        return new TicketResponse
        {
            Id = t.Id,
            TicketNumber = t.TicketNumber,
            ServiceType = t.ServiceType,
            TableCode = t.TableCode,
            Status = t.Status,
            Subtotal = calculatedAmounts.Subtotal,
            TaxRate = t.TaxRate,
            TaxAmount = calculatedAmounts.TaxAmount,
            TotalAmount = calculatedAmounts.TotalAmount,
            CreatedAt = t.CreatedAt,
            PaidAt = t.PaidAt,
            CancelledAt = t.CancelledAt,
            Items = t.Items.Select(i => new TicketItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Producto",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal,
                Status = i.Status,
                Notes = i.Notes
            }).ToList(),
            Payments = t.Payments.Select(p => new PaymentResponse
            {
                Id = p.Id,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                CreatedAt = p.CreatedAt
            }).ToList()
        };
    }

    private static (decimal Subtotal, decimal TaxAmount, decimal TotalAmount) CalculateTicketAmounts(
        IEnumerable<TicketItem> items,
        decimal taxRate)
    {
        var activeSubtotal = items
            .Where(item => !string.Equals(item.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            .Sum(item => item.Subtotal > 0 ? item.Subtotal : item.UnitPrice * item.Quantity);

        var subtotal = RoundMoney(activeSubtotal);
        var taxAmount = RoundMoney(subtotal * (taxRate / 100m));
        var totalAmount = RoundMoney(subtotal + taxAmount);

        return (subtotal, taxAmount, totalAmount);
    }

    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}