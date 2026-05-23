using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class SaleDetail
{
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    public Guid ProductCen { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Sale Sale { get; set; } = null!;
}
