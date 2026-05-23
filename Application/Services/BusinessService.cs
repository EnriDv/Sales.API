using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Shared.Core.Exceptions;

namespace Sales.API.Application.Services;

public class BusinessService : IBusinessService
{
    public Task<CustomerResponse> CreateCustomerAsync(int companyId, CreateCustomerRequest request) => throw new NotImplementedException();
    public Task<VendorResponse> CreateVendorAsync(int companyId, CreateVendorRequest request) => throw new NotImplementedException();
    public Task DeleteCustomerAsync(int companyId, int customerId) => throw new NotImplementedException();
    public Task DeleteVendorAsync(int companyId, int vendorId) => throw new NotImplementedException();
    public Task<List<CustomerResponse>> GetCustomersAsync(int companyId) => throw new NotImplementedException();
    public Task<SalesSettingResponse> GetSettingsAsync(int companyId) => throw new NotImplementedException();
    public Task<List<VendorResponse>> GetVendorsAsync(int companyId) => throw new NotImplementedException();
    public Task UpdateCustomerAsync(int companyId, int customerId, CreateCustomerRequest request) => throw new NotImplementedException();
    public Task UpdateSettingsAsync(int companyId, UpdateSalesSettingRequest request) => throw new NotImplementedException();
    public Task UpdateVendorAsync(int companyId, int vendorId, CreateVendorRequest request) => throw new NotImplementedException();
}