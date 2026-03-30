using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Sales.API.Domain.Entities;
using Shared.Core.Exceptions; // Usamos la excepción global

namespace Sales.API.Application.Services;

public class BusinessService : IBusinessService
{
    private readonly IUnitOfWork _uow;

    public BusinessService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // --- CUSTOMERS ---
    public async Task<List<CustomerResponse>> GetCustomersAsync(int companyId)
    {
        var customers = await _uow.Customers.GetAllAsync(c => c.CompanyId == companyId && c.Active);
        return customers.Select(c => new CustomerResponse
        {
            Id = c.Id, Name = c.Name, Phone = c.Phone, Email = c.Email, Address = c.Address, Active = c.Active
        }).ToList();
    }

    public async Task<CustomerResponse> CreateCustomerAsync(int companyId, CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            CompanyId = companyId,
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.Customers.AddAsync(customer);
        await _uow.SaveAsync();

        return new CustomerResponse { Id = customer.Id, Name = customer.Name, Phone = customer.Phone, Email = customer.Email, Address = customer.Address, Active = customer.Active };
    }

    public async Task UpdateCustomerAsync(int companyId, int customerId, CreateCustomerRequest request)
    {
        var customer = await _uow.Customers.GetByIdAsync(customerId);
        if (customer == null || customer.CompanyId != companyId) throw new NotFoundException("Cliente no encontrado.");

        customer.Name = request.Name;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.Address = request.Address;
        customer.UpdatedAt = DateTime.UtcNow;

        _uow.Customers.Update(customer);
        await _uow.SaveAsync();
    }

    public async Task DeleteCustomerAsync(int companyId, int customerId)
    {
        var customer = await _uow.Customers.GetByIdAsync(customerId);
        if (customer == null || customer.CompanyId != companyId) throw new NotFoundException("Cliente no encontrado.");

        customer.Active = false; // Soft Delete
        customer.UpdatedAt = DateTime.UtcNow;

        _uow.Customers.Update(customer);
        await _uow.SaveAsync();
    }

    // --- VENDORS ---
    public async Task<List<VendorResponse>> GetVendorsAsync(int companyId)
    {
        var vendors = await _uow.Vendors.GetAllAsync(v => v.CompanyId == companyId && v.Active);
        return vendors.Select(v => new VendorResponse
        {
            Id = v.Id, Name = v.Name, Phone = v.Phone, IsWaiter = v.IsWaiter, Active = v.Active
        }).ToList();
    }

    public async Task<VendorResponse> CreateVendorAsync(int companyId, CreateVendorRequest request)
    {
        var vendor = new Vendor
        {
            CompanyId = companyId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            IsWaiter = request.IsWaiter,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.Vendors.AddAsync(vendor);
        await _uow.SaveAsync();

        return new VendorResponse { Id = vendor.Id, Name = vendor.Name, Phone = vendor.Phone, IsWaiter = vendor.IsWaiter, Active = vendor.Active };
    }

    public async Task UpdateVendorAsync(int companyId, int vendorId, CreateVendorRequest request)
    {
        var vendor = await _uow.Vendors.GetByIdAsync(vendorId);
        if (vendor == null || vendor.CompanyId != companyId) throw new NotFoundException("Vendedor no encontrado.");

        vendor.Name = request.Name;
        vendor.Email = request.Email;
        vendor.Phone = request.Phone;
        vendor.IsWaiter = request.IsWaiter;
        vendor.UpdatedAt = DateTime.UtcNow;

        _uow.Vendors.Update(vendor);
        await _uow.SaveAsync();
    }

    public async Task DeleteVendorAsync(int companyId, int vendorId)
    {
        var vendor = await _uow.Vendors.GetByIdAsync(vendorId);
        if (vendor == null || vendor.CompanyId != companyId) throw new NotFoundException("Vendedor no encontrado.");

        vendor.Active = false;
        vendor.UpdatedAt = DateTime.UtcNow;

        _uow.Vendors.Update(vendor);
        await _uow.SaveAsync();
    }

    // --- SETTINGS ---
    public async Task<SalesSettingResponse> GetSettingsAsync(int companyId)
    {
        var settingsList = await _uow.SalesSettings.GetAllAsync(s => s.CompanyId == companyId);
        var settings = settingsList.FirstOrDefault();

        if (settings == null)
            return new SalesSettingResponse { Id = 0, TaxRate = 0, PaymentMethods = "CASH,QR,CARD" };

        return new SalesSettingResponse { Id = settings.Id, TaxRate = settings.TaxRate, PaymentMethods = settings.PaymentMethods };
    }

    public async Task UpdateSettingsAsync(int companyId, UpdateSalesSettingRequest request)
    {
        var settingsList = await _uow.SalesSettings.GetAllAsync(s => s.CompanyId == companyId);
        var settings = settingsList.FirstOrDefault();

        if (settings == null)
        {
            settings = new SalesSetting { CompanyId = companyId, TaxRate = request.TaxRate, PaymentMethods = request.PaymentMethods, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Active = true };
            await _uow.SalesSettings.AddAsync(settings);
        }
        else
        {
            settings.TaxRate = request.TaxRate;
            settings.PaymentMethods = request.PaymentMethods;
            settings.UpdatedAt = DateTime.UtcNow;
            _uow.SalesSettings.Update(settings);
        }

        await _uow.SaveAsync();
    }
}