using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class Sale
{
    public int Id { get; set; }

    public Guid Cen { get; set; }

    public decimal SubtotalPrice { get; set; }

    public decimal TaxPrice { get; set; }

    public decimal DiscountPercentage { get; set; }

    public DateTime SaleDatetime { get; set; }

    public int CustomerId { get; set; }

    public int PaymentTypeId { get; set; }

    public int CompanyId { get; set; }

    public Guid CompanyCen { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual PaymentType PaymentType { get; set; } = null!;

    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
}
