using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Sales.API.Domain.Entities;
using Shared.Core.Exceptions;

namespace Sales.API.Application.Services;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _uow;

    public TicketService(IUnitOfWork uow) => _uow = uow;

    // ── Helpers ──────────────────────────────────
    private async Task<int> ResolveCompanyIdAsync(string companyCen)
    {
        if (!int.TryParse(companyCen, out var id))
            throw new ValidationException($"CEN de empresa inválido: {companyCen}");
        var company = await _uow.Companies.GetByIdAsync(id);
        if (company == null)
            throw new NotFoundException($"Empresa no encontrada: {companyCen}");
        return id;
    }

    private async Task<Ticket> ResolveTicketAsync(int companyId, string ticketCen)
    {
        var tickets = await _uow.TicketQueries.GetTicketByCenAsync(companyId, ticketCen);
        return tickets ?? throw new NotFoundException($"Ticket no encontrado: {ticketCen}");
    }

    private async Task<int> GetDefaultLocationIdAsync(int companyId)
    {
        var locs = await _uow.Locations.GetAllAsync(l => l.CompanyId == companyId);
        return locs.FirstOrDefault()?.Id
            ?? throw new DomainException("No hay ubicaciones configuradas para la empresa.");
    }

    // ── LIST ─────────────────────────────────────
    public async Task<List<TicketContractResponse>> GetTicketsAsync(string companyCen, string? status = null)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var tickets = await _uow.TicketQueries.GetAllTicketsAsync(companyId, status);
        return tickets.Select(MapToContractResponse).ToList();
    }

    // ── SINGLE ────────────────────────────────────
    public async Task<TicketContractResponse> GetTicketAsync(string companyCen, string ticketCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var ticket = await ResolveTicketAsync(companyId, ticketCen);
        return MapToContractResponse(ticket);
    }

    public async Task<TicketTotalsContractResponse> GetTicketTotalsAsync(string companyCen, string ticketCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var ticket = await ResolveTicketAsync(companyId, ticketCen);
        var amounts = CalculateTicketAmounts(ticket.Items, ticket.TaxRate);

        return new TicketTotalsContractResponse
        {
            TicketCen = ticket.TicketNumber,
            Subtotal = amounts.Subtotal,
            TaxRate = ticket.TaxRate,
            TaxAmount = amounts.TaxAmount,
            TotalAmount = amounts.TotalAmount,
            ItemCount = ticket.Items.Count(i => !string.Equals(i.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase)),
            Status = ticket.Status
        };
    }

    // ── CREATE ────────────────────────────────────
    public async Task<TicketContractResponse> CreateTicketAsync(string companyCen, CreateTicketContractRequest request)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var locationId = await GetDefaultLocationIdAsync(companyId);

        var settingsList = await _uow.SalesSettings.GetAllAsync(s => s.CompanyId == companyId);
        decimal defaultTaxRate = settingsList.FirstOrDefault()?.TaxRate ?? 0;

        // Resolve waiter by CEN
        int? vendorId = null;
        if (!string.IsNullOrWhiteSpace(request.WaiterCen))
        {
            if (int.TryParse(request.WaiterCen, out var vId))
            {
                var vendor = await _uow.Vendors.GetByIdAsync(vId);
                if (vendor?.CompanyId == companyId) vendorId = vId;
            }
        }

        var ticket = new Ticket
        {
            CompanyId = companyId,
            LocationId = locationId,
            TicketNumber = BuildTicketNumber(),
            VendorId = vendorId,
            ServiceType = request.ServiceType ?? "DINE_IN",
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

        return await GetTicketAsync(companyCen, ticket.TicketNumber);
    }

    // ── ASSIGN WAITER ─────────────────────────────
    public async Task<AssignTicketWaiterContractResponse> AssignWaiterAsync(string companyCen, string ticketCen, AssignWaiterContractRequest request)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var ticket = await ResolveTicketAsync(companyId, ticketCen);
        if (ticket.Status != "OPEN") throw new DomainException("Solo se puede asignar mesero a un ticket abierto.");

        if (!int.TryParse(request.WaiterCen, out var vendorId))
            throw new ValidationException($"CEN de mesero inválido: {request.WaiterCen}");

        var vendor = await _uow.Vendors.GetByIdAsync(vendorId);
        if (vendor == null || vendor.CompanyId != companyId || !vendor.IsWaiter)
            throw new NotFoundException($"Mesero no encontrado: {request.WaiterCen}");

        ticket.VendorId = vendorId;
        ticket.UpdatedAt = DateTime.UtcNow;
        _uow.Tickets.Update(ticket);
        await _uow.SaveAsync();

        return new AssignTicketWaiterContractResponse
        {
            TicketCen = ticket.TicketNumber,
            WaiterCen = vendor.Id.ToString(),
            WaiterName = vendor.Name
        };
    }

    // ── SEND TO KITCHEN ────────────────────────────
    public async Task SendToKitchenAsync(string companyCen, string ticketCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var ticket = await ResolveTicketAsync(companyId, ticketCen);
        if (ticket.Status != "OPEN") throw new DomainException("El ticket no está abierto.");
        if (!ticket.Items.Any(i => i.Status == "PENDING"))
            throw new DomainException("No hay items pendientes por enviar.");

        foreach (var item in ticket.Items.Where(i => i.Status == "PENDING"))
        {
            item.Status = "IN_COMMAND";
            item.UpdatedAt = DateTime.UtcNow;
            _uow.TicketItems.Update(item);
        }
        ticket.UpdatedAt = DateTime.UtcNow;
        _uow.Tickets.Update(ticket);
        await _uow.SaveAsync();
    }

    // ── PAY TICKET ────────────────────────────────
    public async Task<PayTicketContractResponse> PayTicketAsync(string companyCen, string ticketCen, PayTicketContractRequest request)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var ticket = await ResolveTicketAsync(companyId, ticketCen);
        if (ticket.Status != "OPEN") throw new DomainException("El ticket no está abierto para cobro.");
        if (!ticket.Items.Any(i => i.Status != "CANCELLED")) throw new DomainException("No se puede cobrar un ticket sin items.");

        var amounts = CalculateTicketAmounts(ticket.Items, ticket.TaxRate);
        ticket.Subtotal = amounts.Subtotal;
        ticket.TaxAmount = amounts.TaxAmount;
        ticket.TotalAmount = amounts.TotalAmount;

        decimal amountToPay = request.Amount.HasValue && request.Amount.Value > 0
            ? request.Amount.Value
            : ticket.TotalAmount;

        var payment = new Payment
        {
            TicketId = ticket.Id,
            PaymentMethod = request.PaymentMethodCen,
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

        return new PayTicketContractResponse
        {
            TicketCen = ticket.TicketNumber,
            Status = "PAID",
            TotalAmount = amountToPay,
            PaymentMethod = request.PaymentMethodCen,
            PaidAt = DateTime.UtcNow
        };
    }

    // ── CANCEL TICKET ─────────────────────────────
    public async Task<CancelTicketContractResponse> CancelTicketAsync(string companyCen, string ticketCen, CancelTicketContractRequest? request = null)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var ticket = await ResolveTicketAsync(companyId, ticketCen);
        if (ticket.Status == "PAID") throw new DomainException("No se puede cancelar un ticket pagado.");

        ticket.Status = "CANCELLED";
        ticket.CancelledAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;
        if (request?.Reason != null)
            ticket.Notes = string.IsNullOrEmpty(ticket.Notes) ? request.Reason : $"{ticket.Notes} | Cancel: {request.Reason}";

        foreach (var item in ticket.Items)
        {
            item.Status = "CANCELLED";
            item.UpdatedAt = DateTime.UtcNow;
            _uow.TicketItems.Update(item);
        }

        _uow.Tickets.Update(ticket);
        await _uow.SaveAsync();

        return new CancelTicketContractResponse
        {
            TicketCen = ticket.TicketNumber,
            Status = ticket.Status
        };
    }

    // ── PRINT TICKET ──────────────────────────────
    public Task PrintTicketAsync(string companyCen, string ticketCen)
    {
        // TODO: Implementar integración con impresora / servicio de impresión
        // Por ahora retornamos éxito (no bloquea el flujo)
        return Task.CompletedTask;
    }

    // ── ITEMS ─────────────────────────────────────
    public async Task<List<TicketItemContractResponse>> GetTicketItemsAsync(string companyCen, string ticketCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var ticket = await ResolveTicketAsync(companyId, ticketCen);
        return ticket.Items.Select(MapItemToContract).ToList();
    }

    public async Task<TicketContractResponse> AddItemAsync(string companyCen, string ticketCen, AddTicketItemContractRequest request)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var ticket = await ResolveTicketAsync(companyId, ticketCen);
        if (ticket.Status != "OPEN") throw new DomainException("El ticket ya está pagado o cancelado.");

        // Resolve product by CEN (Code)
        var products = await _uow.Products.GetAllAsync(p => p.CompanyId == companyId && p.Code == request.ProductCen && p.Active);
        var product = products.FirstOrDefault()
            ?? throw new NotFoundException($"Producto no encontrado: {request.ProductCen}");

        var item = new TicketItem
        {
            TicketId = ticket.Id,
            ProductId = product.Id,
            Quantity = request.Quantity,
            UnitPrice = product.Price,
            Status = "PENDING",
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.TicketItems.AddAsync(item);
        await _uow.SaveAsync();

        return await GetTicketAsync(companyCen, ticketCen);
    }

    public async Task<TicketContractResponse> UpdateItemAsync(string companyCen, string ticketCen, string ticketItemCen, UpdateTicketItemContractRequest request)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var ticket = await ResolveTicketAsync(companyId, ticketCen);
        if (ticket.Status != "OPEN") throw new DomainException("Solo puedes actualizar items en tickets abiertos.");

        if (!int.TryParse(ticketItemCen, out var itemId))
            throw new ValidationException($"CEN de item inválido: {ticketItemCen}");

        var item = ticket.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException($"Item no encontrado: {ticketItemCen}");

        if (request.Quantity.HasValue) item.Quantity = request.Quantity.Value;
        if (request.Notes != null) item.Notes = request.Notes;
        item.UpdatedAt = DateTime.UtcNow;

        _uow.TicketItems.Update(item);
        await _uow.SaveAsync();

        return await GetTicketAsync(companyCen, ticketCen);
    }

    public async Task ResendItemAsync(string companyCen, string ticketCen, string ticketItemCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var ticket = await ResolveTicketAsync(companyId, ticketCen);

        if (!int.TryParse(ticketItemCen, out var itemId))
            throw new ValidationException($"CEN de item inválido: {ticketItemCen}");

        var item = ticket.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException($"Item no encontrado: {ticketItemCen}");

        // Reenviar: volver a PENDING para que KDS lo recoja nuevamente
        item.Status = "PENDING";
        item.UpdatedAt = DateTime.UtcNow;

        _uow.TicketItems.Update(item);
        await _uow.SaveAsync();
    }

    // ── Private helpers ────────────────────────────
    private static string BuildTicketNumber()
    {
        return $"T-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
    }

    private static (decimal Subtotal, decimal TaxAmount, decimal TotalAmount) CalculateTicketAmounts(
        IEnumerable<TicketItem> items, decimal taxRate)
    {
        var subtotal = RoundMoney(items
            .Where(i => !string.Equals(i.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            .Sum(i => i.Subtotal > 0 ? i.Subtotal : i.UnitPrice * i.Quantity));
        var taxAmount = RoundMoney(subtotal * (taxRate / 100m));
        var total = RoundMoney(subtotal + taxAmount);
        return (subtotal, taxAmount, total);
    }

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static TicketContractResponse MapToContractResponse(Ticket t)
    {
        var amounts = CalculateTicketAmounts(t.Items, t.TaxRate);
        return new TicketContractResponse
        {
            TicketCen = t.TicketNumber,
            Status = t.Status,
            ServiceType = t.ServiceType,
            TableCode = t.TableCode,
            WaiterName = t.Vendor?.Name,
            Notes = t.Notes,
            Subtotal = amounts.Subtotal,
            TaxRate = t.TaxRate,
            TaxAmount = amounts.TaxAmount,
            TotalAmount = amounts.TotalAmount,
            CreatedAt = t.CreatedAt,
            PaidAt = t.PaidAt,
            CancelledAt = t.CancelledAt,
            Items = t.Items.Select(MapItemToContract).ToList()
        };
    }

    private static TicketItemContractResponse MapItemToContract(TicketItem i) => new()
    {
        TicketItemCen = i.Id.ToString(),
        ProductCen = i.Product?.Code ?? string.Empty,
        ProductName = i.Product?.Name ?? "Producto",
        Quantity = i.Quantity,
        UnitPrice = i.UnitPrice,
        Subtotal = i.Subtotal > 0 ? i.Subtotal : i.UnitPrice * i.Quantity,
        Status = i.Status,
        Notes = i.Notes
    };
}