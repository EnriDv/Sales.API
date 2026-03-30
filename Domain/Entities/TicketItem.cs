namespace Sales.API.Domain.Entities;

public class TicketItem
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; } 
    public string Status { get; set; } = "PENDING";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}