namespace Sales.API.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public string? PaidBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}