using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Shared.Core.Exceptions;

namespace Sales.API.Application.Services;

public class KdsService : IKdsService
{
    private readonly IUnitOfWork _uow;

    public KdsService(IUnitOfWork uow) => _uow = uow;

    private async Task<int> ResolveCompanyIdAsync(string companyCen)
    {
        if (!int.TryParse(companyCen, out var id))
            throw new ValidationException($"CEN de empresa inválido: {companyCen}");
        var company = await _uow.Companies.GetByIdAsync(id);
        if (company == null)
            throw new NotFoundException($"Empresa no encontrada: {companyCen}");
        return id;
    }

    public async Task<List<KdsTeamContractResponse>> GetTeamsAsync(string companyCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        // By default, return a single team for the whole company, or locations
        var locations = await _uow.Locations.GetAllAsync(l => l.CompanyId == companyId);
        
        return locations.Select(l => new KdsTeamContractResponse
        {
            TeamCen = $"LOC-{l.Id}",
            Name = $"Cocina Principal - {l.Name}",
            Type = "KITCHEN",
            Active = true
        }).ToList();
    }

    public async Task<List<KdsItemContractResponse>> GetTeamItemsAsync(string companyCen, string teamCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        // TeamCen format: "LOC-{id}"
        int locationId = 0;
        if (teamCen.StartsWith("LOC-") && int.TryParse(teamCen.Substring(4), out var id))
        {
            locationId = id;
        }

        // Get ticket items for tickets in the given location that are IN_COMMAND or SERVED (if we want to show history)
        var tickets = await _uow.TicketQueries.GetActiveTicketsAsync(companyId, locationId);
        var items = tickets.SelectMany(t => t.Items)
            .Where(i => i.Status == "IN_COMMAND" || i.Status == "SERVED")
            .OrderBy(i => i.CreatedAt)
            .Select(i => new KdsItemContractResponse
            {
                TicketItemCen = i.Id.ToString(),
                ProductName = i.Product?.Name ?? "Producto",
                Quantity = i.Quantity,
                Status = i.Status == "IN_COMMAND" ? "preparing" : "delivered", // Map to KDS status
                Notes = i.Notes,
                TicketCen = i.Ticket?.TicketNumber ?? string.Empty,
                TableCode = i.Ticket?.TableCode,
                CreatedAt = i.CreatedAt
            })
            .ToList();

        return items;
    }

    public async Task UpdateItemStatusAsync(string companyCen, string ticketItemCen, UpdateKdsItemStatusContractRequest request)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        if (!int.TryParse(ticketItemCen, out var itemId))
            throw new ValidationException($"CEN de item inválido: {ticketItemCen}");

        var item = await _uow.TicketItems.GetByIdAsync(itemId);
        if (item == null) throw new NotFoundException($"Item no encontrado: {ticketItemCen}");

        // mapping KDS status back to TicketItem status
        var mappedStatus = request.Status.ToLower() switch
        {
            "preparing" => "IN_COMMAND",
            "delivered" => "SERVED",
            "canceled" => "CANCELLED",
            _ => throw new ValidationException("Estado de KDS inválido. Use: preparing, delivered, canceled")
        };

        item.Status = mappedStatus;
        item.UpdatedAt = DateTime.UtcNow;

        _uow.TicketItems.Update(item);
        await _uow.SaveAsync();
    }
}
