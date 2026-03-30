namespace Sales.API.Domain.Entities;

public class SalesSetting
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public decimal TaxRate { get; set; }
    public string PaymentMethods { get; set; } = "CASH,QR,CARD";
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;
}