using Microsoft.EntityFrameworkCore;
using Sales.API.Application.Interfaces;
using Sales.API.Domain.Entities;
using Sales.API.Infrastructure.Persistence;

namespace Sales.API.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly SalesDbContext _ctx;
    public TicketRepository(SalesDbContext ctx) => _ctx = ctx;

    public async Task<Ticket?> GetTicketWithDetailsAsync(int companyId, int ticketId)
    {
        return await _ctx.Tickets
            .Include(t => t.Items).ThenInclude(i => i.Product)
            .Include(t => t.Vendor)
            .Include(t => t.Payments)
            .FirstOrDefaultAsync(t => t.CompanyId == companyId && t.Id == ticketId);
    }

    public async Task<Ticket?> GetTicketByCenAsync(int companyId, string ticketCen)
    {
        return await _ctx.Tickets
            .Include(t => t.Items).ThenInclude(i => i.Product)
            .Include(t => t.Vendor)
            .Include(t => t.Payments)
            .FirstOrDefaultAsync(t => t.CompanyId == companyId && t.TicketNumber == ticketCen);
    }

    public async Task<List<Ticket>> GetAllTicketsAsync(int companyId, string? status = null)
    {
        var query = _ctx.Tickets
            .Include(t => t.Items).ThenInclude(i => i.Product)
            .Include(t => t.Vendor)
            .Include(t => t.Payments)
            .Where(t => t.CompanyId == companyId);

        if (status != null)
            query = query.Where(t => t.Status == status);

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<List<Ticket>> GetActiveTicketsAsync(int companyId, int locationId)
    {
        return await _ctx.Tickets
            .Include(t => t.Items).ThenInclude(i => i.Product)
            .Include(t => t.Payments)
            .Where(t => t.CompanyId == companyId && t.LocationId == locationId && t.Status == "OPEN")
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Ticket>> GetTicketHistoryAsync(int companyId, int locationId, int take)
    {
        var safeTake = Math.Clamp(take, 1, 500);

        return await _ctx.Tickets
            .Include(t => t.Items).ThenInclude(i => i.Product)
            .Include(t => t.Payments)
            .Where(t => t.CompanyId == companyId && t.LocationId == locationId && t.Status != "OPEN")
            .OrderByDescending(t => t.UpdatedAt)
            .Take(safeTake)
            .ToListAsync();
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly SalesDbContext _context;

    public IGenericRepository<Company> Companies { get; }
    public IGenericRepository<Location> Locations { get; }
    public IGenericRepository<Product> Products { get; }
    public IGenericRepository<SalesSetting> SalesSettings { get; }
    public IGenericRepository<Vendor> Vendors { get; }
    public IGenericRepository<Customer> Customers { get; }
    public IGenericRepository<Ticket> Tickets { get; }
    public IGenericRepository<TicketItem> TicketItems { get; }
    public IGenericRepository<Payment> Payments { get; }
    public ITicketRepository TicketQueries { get; }

    public UnitOfWork(SalesDbContext context)
    {
        _context = context;
        Companies = new GenericRepository<Company>(_context);
        Locations = new GenericRepository<Location>(_context);
        Products = new GenericRepository<Product>(_context);
        SalesSettings = new GenericRepository<SalesSetting>(_context);
        Vendors = new GenericRepository<Vendor>(_context);
        Customers = new GenericRepository<Customer>(_context);
        Tickets = new GenericRepository<Ticket>(_context);
        TicketItems = new GenericRepository<TicketItem>(_context);
        Payments = new GenericRepository<Payment>(_context);
        TicketQueries = new TicketRepository(_context);
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}