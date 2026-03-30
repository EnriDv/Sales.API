using Sales.API.Application.DTOs;

namespace Sales.API.Application.Interfaces;

public interface IBusinessService
{
    // Customers
    Task<List<CustomerResponse>> GetCustomersAsync(int companyId);
    Task<CustomerResponse> CreateCustomerAsync(int companyId, CreateCustomerRequest request);
    Task UpdateCustomerAsync(int companyId, int customerId, CreateCustomerRequest request);
    Task DeleteCustomerAsync(int companyId, int customerId);

    // Vendors
    Task<List<VendorResponse>> GetVendorsAsync(int companyId);
    Task<VendorResponse> CreateVendorAsync(int companyId, CreateVendorRequest request);
    Task UpdateVendorAsync(int companyId, int vendorId, CreateVendorRequest request);
    Task DeleteVendorAsync(int companyId, int vendorId);

    // Settings
    Task<SalesSettingResponse> GetSettingsAsync(int companyId);
    Task UpdateSettingsAsync(int companyId, UpdateSalesSettingRequest request);
}

public interface ITicketService
{
    Task<List<TicketResponse>> GetActiveTicketsAsync(int companyId, int locationId);
    Task<List<TicketResponse>> GetTicketHistoryAsync(int companyId, int locationId, int take = 100);
    Task<TicketResponse> GetTicketAsync(int companyId, int ticketId);
    Task<TicketResponse> CreateTicketAsync(int companyId, CreateTicketRequest request);
    Task<TicketResponse> AddItemAsync(int companyId, int ticketId, AddTicketItemRequest request);
    Task<TicketResponse> UpdateItemStatusAsync(int companyId, int ticketId, int itemId, UpdateTicketItemStatusRequest request);
    Task<TicketResponse> CheckoutAsync(int companyId, int ticketId, CheckoutRequest request);
    Task CancelTicketAsync(int companyId, int ticketId);
}