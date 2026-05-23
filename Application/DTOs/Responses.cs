using System.Text.Json.Serialization;

namespace Sales.API.Application.DTOs;

public class TicketContractResponse
{
    public string TicketCen { get; set; } = string.Empty;
    public int DailyNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? WaiterCen { get; set; }
    public string? CompanyCen { get; set; }
    public decimal TaxAmount { get; set; }
    
    public decimal Subtotal { get; set; }
    public decimal TotalAmount { get; set; }
    public List<TicketItemContractResponse> Items { get; set; } = new();
}

public class TicketItemContractResponse
{
    public string TicketItemCen { get; set; } = string.Empty;
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public int ResendCount { get; set; }
}

public class TicketTotalsContractResponse
{
    public string TicketCen { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}

public class PayTicketContractResponse
{
    public string SaleCen { get; set; } = string.Empty;
    public string TicketCen { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? InventoryDocumentCen { get; set; }
}

public class ProcessRestaurantOrderPaymentResultDto
{
    public bool IsSuccess { get; set; }
    public int? SaleId { get; set; }
    public string? SaleCen { get; set; }
    public string? InventoryDocumentCen { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<StockInsufficiencyResponseDto> Insufficiencies { get; set; } = new();
}

public class StockInsufficiencyResponseDto
{
    public int ProductId { get; set; }
    public string? ProductCen { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? WarehouseCen { get; set; }
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int MissingQuantity { get; set; }
}

public class CancelTicketContractResponse
{
    public string TicketCen { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class AssignTicketWaiterContractResponse
{
    public string TicketCen { get; set; } = string.Empty;
    public string WaiterCen { get; set; } = string.Empty;
    public string WaiterName { get; set; } = string.Empty;
}

public class KdsTeamContractResponse
{
    public string TeamCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> CategoryCens { get; set; } = new();
}

public class KdsItemContractResponse
{
    public string TicketItemCen { get; set; } = string.Empty;
    public string TicketCen { get; set; } = string.Empty;
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int ResendCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DailySalesDashboardDto
{
    public decimal TotalSales { get; set; }
    public int TicketsCount { get; set; }
    public decimal AverageTicket { get; set; }
}

public class TopProductDashboardContractResponse
{
    public string? ProductCen { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public string? CategoryCen { get; set; }
    public string? CategoryName { get; set; }
    public decimal SalePrice { get; set; }
}

public class KdsStatusDashboardDto
{
    public int PendingCount { get; set; }
    public int PreparingCount { get; set; }
    public int ReadyCount { get; set; }
}

public class TaxConfigurationContractResponse
{
    public string CompanyCen { get; set; } = string.Empty;
    public decimal GlobalTaxPercentage { get; set; }
}

public class PaymentMethodContractResponse
{
    public string PaymentMethodCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class WaiterContractResponse
{
    public string WaiterCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class SellableProductContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryCen { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? WarehouseCen { get; set; }
    public decimal SalePrice { get; set; }
    public decimal AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public string? StationCode { get; set; }
}

public class ProductLookupContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public string? CategoryCen { get; set; }
    public string? CategoryName { get; set; }
}

public class CustomerResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool Active { get; set; }
}

public class VendorResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsWaiter { get; set; }
    public bool Active { get; set; }
}

public class SalesSettingResponse
{
    public int Id { get; set; }
    public decimal TaxRate { get; set; }
    public string PaymentMethods { get; set; } = string.Empty;
}

public class TicketResponse
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string? TableCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<TicketItemResponse> Items { get; set; } = new();
    public List<PaymentResponse> Payments { get; set; } = new();
}

public class TicketItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class PaymentResponse
{
    public int Id { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}