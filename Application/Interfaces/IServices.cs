using Sales.API.Application.DTOs;

namespace Sales.API.Application.Interfaces;

// ────────────────────────────────────────────
// TICKETS — Contrato v1
// ────────────────────────────────────────────
public interface ITicketService
{
    // List
    Task<List<TicketContractResponse>> GetTicketsAsync(string companyCen, string? status = null);

    // Single ticket
    Task<TicketContractResponse> GetTicketAsync(string companyCen, string ticketCen);
    Task<TicketTotalsContractResponse> GetTicketTotalsAsync(string companyCen, string ticketCen);

    // Lifecycle
    Task<TicketContractResponse> CreateTicketAsync(string companyCen, CreateTicketContractRequest request);
    Task<AssignTicketWaiterContractResponse> AssignWaiterAsync(string companyCen, string ticketCen, AssignWaiterContractRequest request);
    Task SendToKitchenAsync(string companyCen, string ticketCen);
    Task<PayTicketContractResponse> PayTicketAsync(string companyCen, string ticketCen, PayTicketContractRequest request);
    Task<CancelTicketContractResponse> CancelTicketAsync(string companyCen, string ticketCen, CancelTicketContractRequest? request = null);
    Task PrintTicketAsync(string companyCen, string ticketCen);

    // Items
    Task<List<TicketItemContractResponse>> GetTicketItemsAsync(string companyCen, string ticketCen);
    Task<TicketContractResponse> AddItemAsync(string companyCen, string ticketCen, AddTicketItemContractRequest request);
    Task<TicketContractResponse> UpdateItemAsync(string companyCen, string ticketCen, string ticketItemCen, UpdateTicketItemContractRequest request);
    Task ResendItemAsync(string companyCen, string ticketCen, string ticketItemCen);
}

// ────────────────────────────────────────────
// KDS — Contrato v1
// ────────────────────────────────────────────
public interface IKdsService
{
    Task<List<KdsTeamContractResponse>> GetTeamsAsync(string companyCen);
    Task<List<KdsItemContractResponse>> GetTeamItemsAsync(string companyCen, string teamCen);
    Task UpdateItemStatusAsync(string companyCen, string ticketItemCen, UpdateKdsItemStatusContractRequest request);
}

// ────────────────────────────────────────────
// DASHBOARD — Contrato v1
// ────────────────────────────────────────────
public interface ISalesDashboardService
{
    Task<DailySalesDashboardContractDto> GetDailySalesAsync(string companyCen, DateTime? date = null);
    Task<List<TopProductDashboardContractDto>> GetTopProductsAsync(string companyCen, int topN = 10);
    Task<KdsDashboardStatusContractDto> GetKdsStatusAsync(string companyCen);
}

// ────────────────────────────────────────────
// TAX CONFIGURATION — Contrato v1
// ────────────────────────────────────────────
public interface ITaxConfigurationService
{
    Task<TaxConfigurationContractResponse> GetTaxConfigurationAsync(string companyCen);
    Task<TaxConfigurationContractResponse> UpdateTaxConfigurationAsync(string companyCen, UpdateTaxConfigurationContractRequest request);
}

// ────────────────────────────────────────────
// PAYMENT METHODS — Contrato v1
// ────────────────────────────────────────────
public interface IPaymentMethodService
{
    Task<List<PaymentMethodContractResponse>> GetPaymentMethodsAsync();
}

// ────────────────────────────────────────────
// WAITERS — Contrato v1
// ────────────────────────────────────────────
public interface IWaiterService
{
    Task<List<WaiterContractResponse>> GetWaitersAsync(string companyCen);
}

// ────────────────────────────────────────────
// SALES CATALOG — Contrato v1
// ────────────────────────────────────────────
public interface ISalesCatalogService
{
    Task<List<SellableProductContractDto>> GetSellableProductsAsync(
        string companyCen,
        string? search = null,
        string? categoryCen = null,
        string? warehouseCen = null,
        bool onlyAvailable = true,
        int page = 1,
        int pageSize = 50);
}

// ────────────────────────────────────────────
// LEGACY — Internal admin endpoints
// ────────────────────────────────────────────
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