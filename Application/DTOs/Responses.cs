using System.Text.Json.Serialization;

namespace Sales.API.Application.DTOs;

// ── TICKETS CONTRACT ─────────────────────────
public class TicketContractResponse
{
    public string TicketCen { get; set; } = string.Empty;  // TicketNumber
    public string Status { get; set; } = string.Empty;
    public string? ServiceType { get; set; }
    public string? TableCode { get; set; }
    public string? WaiterName { get; set; }
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<TicketItemContractResponse> Items { get; set; } = new();
}

public class TicketItemContractResponse
{
    public string TicketItemCen { get; set; } = string.Empty; // Id.ToString()
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("note")]
    public string? Notes { get; set; }
}

public class TicketTotalsContractResponse
{
    public string TicketCen { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PayTicketContractResponse
{
    public string TicketCen { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
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

// ── KDS CONTRACT ─────────────────────────────
public class KdsTeamContractResponse
{
    public string TeamCen { get; set; } = string.Empty;  // command_station.code
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;    // station_type
    public bool Active { get; set; }
}

public class KdsItemContractResponse
{
    public string TicketItemCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string TicketCen { get; set; } = string.Empty;
    public string? TableCode { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── DASHBOARD CONTRACT ───────────────────────
public class DailySalesDashboardContractDto
{
    [JsonIgnore]
    public DateTime Date { get; set; }

    public decimal TotalSales { get; set; }

    [JsonPropertyName("ticketsCount")]
    public int TicketsCount { get; set; }

    public decimal AverageTicket { get; set; }
}

public class TopProductDashboardContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
}

public class KdsDashboardStatusContractDto
{
    public int Pending { get; set; }
    public int Preparing { get; set; }
    public int Delivered { get; set; }
    public int Total { get; set; }
}

// ── TAX CONFIGURATION CONTRACT ───────────────
public class TaxConfigurationContractResponse
{
    public string CompanyCen { get; set; } = string.Empty;

    [JsonPropertyName("globalTaxPercentage")]
    public decimal GlobalTaxPercentage { get; set; }
}

// ── PAYMENT METHODS CONTRACT ─────────────────
public class PaymentMethodContractResponse
{
    public string PaymentMethodCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

// ── WAITERS CONTRACT ─────────────────────────
public class WaiterContractResponse
{
    public string WaiterCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

// ── CATALOG (Sellable Products) CONTRACT ─────
public class SellableProductContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryCen { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("salePrice")]
    public decimal SalePrice { get; set; }

    [JsonPropertyName("availableQuantity")]
    public decimal AvailableQuantity { get; set; }

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    public string? StationCode { get; set; }
}

// ── LEGACY (kept for TicketService backward compat) ──
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