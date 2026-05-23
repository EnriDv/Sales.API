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

public interface IUnitOfWork
{
    IGenericRepository<Customer> Customers { get; }
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<OrderDetail> OrderDetails { get; }
    IGenericRepository<OrderStatus> OrderStatuses { get; }
    IGenericRepository<PaymentType> PaymentTypes { get; }
    IGenericRepository<RestaurantOrder> RestaurantOrders { get; }
    IGenericRepository<RestaurantOrderDetail> RestaurantOrderDetails { get; }
    IGenericRepository<RestaurantOrderDetailStatus> RestaurantOrderDetailStatuses { get; }
    IGenericRepository<Sale> Sales { get; }
    IGenericRepository<SaleDetail> SaleDetails { get; }
    IGenericRepository<TaxConfiguration> TaxConfigurations { get; }
    IGenericRepository<Team> Teams { get; }
    IGenericRepository<TeamConfiguration> TeamConfigurations { get; }
    IGenericRepository<Waiter> Waiters { get; }
    IGenericRepository<WarehouseConfiguration> WarehouseConfigurations { get; }

    Task SaveAsync();
}