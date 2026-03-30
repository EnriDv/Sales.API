namespace Sales.API.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int LocationId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public int? VendorId { get; set; }
    public int? CustomerId { get; set; }
    public string ServiceType { get; set; } = "DINE_IN"; // DINE_IN, TAKEAWAY, DELIVERY
    public string? TableCode { get; set; }
    public string Status { get; set; } = "OPEN"; // OPEN, PAID, CANCELLED
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
    public virtual Vendor? Vendor { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<TicketItem> Items { get; set; } = new List<TicketItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}