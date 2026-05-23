using Sales.API.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Sales.API.Application.Interfaces;

public interface ITicketService
{
    Task<List<TicketContractResponse>> GetTicketsAsync(string companyCen, string? status = null);

    Task<TicketContractResponse> GetTicketAsync(string companyCen, string ticketCen);
    Task<TicketTotalsContractResponse> GetTicketTotalsAsync(string companyCen, string ticketCen);

    Task<TicketContractResponse> CreateTicketAsync(string companyCen, CreateTicketContractRequest request);
    Task<AssignTicketWaiterContractResponse> AssignWaiterAsync(string companyCen, string ticketCen, AssignTicketWaiterContractRequest request);
    Task SendToKitchenAsync(string companyCen, string ticketCen);
    Task<PayTicketContractResponse> PayTicketAsync(string companyCen, string ticketCen, PayTicketContractRequest request);
    Task<CancelTicketContractResponse> CancelTicketAsync(string companyCen, string ticketCen, CancelTicketContractRequest? request = null);
    Task<FileContentResult> PrintTicketAsync(string companyCen, string ticketCen);

    Task<List<TicketItemContractResponse>> GetTicketItemsAsync(string companyCen, string ticketCen);
    Task<TicketContractResponse> AddItemAsync(string companyCen, string ticketCen, CreateTicketItemContractRequest request);
    Task<TicketContractResponse> UpdateItemAsync(string companyCen, string ticketCen, string ticketItemCen, UpdateTicketItemContractRequest request);
    Task ResendItemAsync(string companyCen, string ticketCen, string ticketItemCen);
}

public interface IKdsService
{
    Task<List<KdsTeamContractResponse>> GetTeamsAsync(string companyCen);
    Task<List<KdsItemContractResponse>> GetTeamItemsAsync(string companyCen, string teamCen);
    Task UpdateItemStatusAsync(string companyCen, string ticketItemCen, UpdateKdsItemStatusContractRequest request);
}

public interface ISalesDashboardService
{
    Task<DailySalesDashboardDto> GetDailySalesAsync(string companyCen, DateTime? date = null);
    Task<List<TopProductDashboardContractResponse>> GetTopProductsAsync(string companyCen, int topN = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<KdsStatusDashboardDto> GetKdsStatusAsync(string companyCen);
}

public interface ITaxConfigurationService
{
    Task<TaxConfigurationContractResponse> GetTaxConfigurationAsync(string companyCen);
    Task<TaxConfigurationContractResponse> UpdateTaxConfigurationAsync(string companyCen, UpdateTaxConfigurationContractRequest request);
}

public interface IPaymentMethodService
{
    Task<List<PaymentMethodContractResponse>> GetPaymentMethodsAsync();
}

public interface IWaiterService
{
    Task<List<WaiterContractResponse>> GetWaitersAsync(string companyCen);
}

public interface IInventoryApiClient
{
    Task<List<SellableProductContractDto>> GetSellableProductsAsync(
        string companyCen,
        string? search = null,
        string? categoryCen = null,
        string? warehouseCen = null,
        bool onlyAvailable = true,
        int page = 1,
        int pageSize = 50);

    Task<Dictionary<string, ProductLookupContractDto>> GetProductLookupMapAsync(string companyCen, IEnumerable<string> productCens);

    Task ConsumeStockAsync(string companyCen, ConsumeStockContractRequest request);
}

[System.Obsolete("Use IInventoryApiClient instead.")]
public interface IInventoryProductLookupService : IInventoryApiClient
{
}

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

public interface IBusinessService
{
    Task<List<CustomerResponse>> GetCustomersAsync(int companyId);
    Task<CustomerResponse> CreateCustomerAsync(int companyId, CreateCustomerRequest request);
    Task UpdateCustomerAsync(int companyId, int customerId, CreateCustomerRequest request);
    Task DeleteCustomerAsync(int companyId, int customerId);

    Task<List<VendorResponse>> GetVendorsAsync(int companyId);
    Task<VendorResponse> CreateVendorAsync(int companyId, CreateVendorRequest request);
    Task UpdateVendorAsync(int companyId, int vendorId, CreateVendorRequest request);
    Task DeleteVendorAsync(int companyId, int vendorId);

    Task<SalesSettingResponse> GetSettingsAsync(int companyId);
    Task UpdateSettingsAsync(int companyId, UpdateSalesSettingRequest request);
}