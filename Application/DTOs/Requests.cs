using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Sales.API.Application.DTOs;

// ── TICKETS ─────────────────────────────────
public class CreateTicketContractRequest
{
    public string? WaiterCen { get; set; }      // VendorId lookup by code
    public string? ServiceType { get; set; } = "DINE_IN";
    public string? TableCode { get; set; }
    public string? Notes { get; set; }
}

public class AddTicketItemContractRequest
{
    [Required] public string ProductCen { get; set; } = string.Empty;
    [Required, Range(0.001, double.MaxValue)] public decimal Quantity { get; set; } = 1;

    [JsonPropertyName("note")]
    public string? Notes { get; set; }
}

public class UpdateTicketItemContractRequest
{
    public decimal? Quantity { get; set; }

    [JsonPropertyName("note")]
    public string? Notes { get; set; }
}

public class AssignWaiterContractRequest
{
    [Required] public string WaiterCen { get; set; } = string.Empty;
}

public class CancelTicketContractRequest
{
    public string? Reason { get; set; }
}

public class PayTicketContractRequest
{
    [Required] public string PaymentMethodCen { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string? Reference { get; set; }
    public string? PaidBy { get; set; }
}

// ── KDS ──────────────────────────────────────
public class UpdateKdsItemStatusContractRequest
{
    /// <summary>preparing | delivered | canceled</summary>
    [Required] public string Status { get; set; } = string.Empty;
}

// ── TAX CONFIGURATION ────────────────────────
public class UpdateTaxConfigurationContractRequest
{
    [Required, Range(0, 100)]
    [JsonPropertyName("globalTaxPercentage")]
    public decimal GlobalTaxPercentage { get; set; }
}

// ── LEGACY — kept for internal use ───────────
public class CreateCustomerRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public class CreateVendorRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsWaiter { get; set; } = true;
}

public class UpdateSalesSettingRequest
{
    public decimal TaxRate { get; set; } = 0;
    public string PaymentMethods { get; set; } = "CASH,QR,CARD";
}

public class CreateTicketRequest
{
    [Required] public int LocationId { get; set; }
    public int? VendorId { get; set; }
    public int? CustomerId { get; set; }
    public string ServiceType { get; set; } = "DINE_IN";
    public string? TableCode { get; set; }
    public string? Notes { get; set; }
}

public class AddTicketItemRequest
{
    [Required] public int ProductId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; } = 0;
    public string? Notes { get; set; }
}

public class UpdateTicketItemStatusRequest
{
    [Required] public string Status { get; set; } = string.Empty;
}

public class CheckoutRequest
{
    [Required] public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 0;
    public string? Reference { get; set; }
    public string? PaidBy { get; set; }
}