using Sales.API.Domain.Entities;
using System.Linq.Expressions;

namespace Sales.API.Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface ITicketRepository
{
    Task<Ticket?> GetTicketWithDetailsAsync(int companyId, int ticketId);
    Task<List<Ticket>> GetActiveTicketsAsync(int companyId, int locationId);
    Task<List<Ticket>> GetTicketHistoryAsync(int companyId, int locationId, int take);
}

public interface IUnitOfWork
{
    IGenericRepository<Company> Companies { get; }
    IGenericRepository<Location> Locations { get; }
    IGenericRepository<Product> Products { get; }
    IGenericRepository<SalesSetting> SalesSettings { get; }
    IGenericRepository<Vendor> Vendors { get; }
    IGenericRepository<Customer> Customers { get; }
    IGenericRepository<Ticket> Tickets { get; }
    IGenericRepository<TicketItem> TicketItems { get; }
    IGenericRepository<Payment> Payments { get; }
    
    ITicketRepository TicketQueries { get; }
    
    Task SaveAsync();
}