using System.ComponentModel.DataAnnotations;

namespace Sales.API.Application.DTOs;

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
    public string ServiceType { get; set; } = "DINE_IN"; // DINE_IN, TAKEAWAY, DELIVERY
    public string? TableCode { get; set; }
    public string? Notes { get; set; }
}

public class AddTicketItemRequest
{
    [Required] public int ProductId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; } = 0; // Si viene en 0, el servicio buscará el precio del producto
    public string? Notes { get; set; }
}

public class UpdateTicketItemStatusRequest
{
    [Required] public string Status { get; set; } = string.Empty; // PENDING, IN_COMMAND, SERVED, CANCELLED
}

public class CheckoutRequest
{
    [Required] public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 0;
    public string? Reference { get; set; }
    public string? PaidBy { get; set; }
}