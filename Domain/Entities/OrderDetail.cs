using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public Guid ProductCen { get; set; }

    public int Quantity { get; set; }

    public decimal ProductPrice { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public int OrderModelId { get; set; }

    public virtual Order Order { get; set; } = null!;
}
