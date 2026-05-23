using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Sales.API.Application.DTOs;

public class CreateTicketContractRequest
{
    public string? WaiterCen { get; set; }
}

public class CreateTicketItemContractRequest
{
    [Required] public string ProductCen { get; set; } = string.Empty;
    [Required, Range(1, int.MaxValue)] public int Quantity { get; set; } = 1;
    public string? Note { get; set; }
}

public class UpdateTicketItemContractRequest
{
    public int? Quantity { get; set; }
    public string? Note { get; set; }
}

public class AssignTicketWaiterContractRequest
{
    [Required] public string WaiterCen { get; set; } = string.Empty;
}

public class CancelTicketContractRequest
{
    public string? Reason { get; set; }
}

public class PayTicketContractRequest
{
    [Required] public string PaymentMethodCode { get; set; } = string.Empty;
}

public class ProductLookupContractRequest
{
    [Required]
    public List<string> ProductCens { get; set; } = new();
}

public class UpdateKdsItemStatusContractRequest
{
    [Required] public string Status { get; set; } = string.Empty;
}

public class UpdateTaxConfigurationContractRequest
{
    [Required, Range(0, 100)]
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