using Microsoft.EntityFrameworkCore;
using Sales.API.Application.Interfaces;
using Sales.API.Domain.Entities;
using Sales.API.Infrastructure.Persistence;

namespace Sales.API.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SalesDbContext _context;

    public IGenericRepository<Customer> Customers { get; }
    public IGenericRepository<Order> Orders { get; }
    public IGenericRepository<OrderDetail> OrderDetails { get; }
    public IGenericRepository<OrderStatus> OrderStatuses { get; }
    public IGenericRepository<PaymentType> PaymentTypes { get; }
    public IGenericRepository<RestaurantOrder> RestaurantOrders { get; }
    public IGenericRepository<RestaurantOrderDetail> RestaurantOrderDetails { get; }
    public IGenericRepository<RestaurantOrderDetailStatus> RestaurantOrderDetailStatuses { get; }
    public IGenericRepository<Sale> Sales { get; }
    public IGenericRepository<SaleDetail> SaleDetails { get; }
    public IGenericRepository<TaxConfiguration> TaxConfigurations { get; }
    public IGenericRepository<Team> Teams { get; }
    public IGenericRepository<TeamConfiguration> TeamConfigurations { get; }
    public IGenericRepository<Waiter> Waiters { get; }
    public IGenericRepository<WarehouseConfiguration> WarehouseConfigurations { get; }

    public UnitOfWork(SalesDbContext context)
    {
        _context = context;
        Customers = new GenericRepository<Customer>(_context);
        Orders = new GenericRepository<Order>(_context);
        OrderDetails = new GenericRepository<OrderDetail>(_context);
        OrderStatuses = new GenericRepository<OrderStatus>(_context);
        PaymentTypes = new GenericRepository<PaymentType>(_context);
        RestaurantOrders = new GenericRepository<RestaurantOrder>(_context);
        RestaurantOrderDetails = new GenericRepository<RestaurantOrderDetail>(_context);
        RestaurantOrderDetailStatuses = new GenericRepository<RestaurantOrderDetailStatus>(_context);
        Sales = new GenericRepository<Sale>(_context);
        SaleDetails = new GenericRepository<SaleDetail>(_context);
        TaxConfigurations = new GenericRepository<TaxConfiguration>(_context);
        Teams = new GenericRepository<Team>(_context);
        TeamConfigurations = new GenericRepository<TeamConfiguration>(_context);
        Waiters = new GenericRepository<Waiter>(_context);
        WarehouseConfigurations = new GenericRepository<WarehouseConfiguration>(_context);
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}