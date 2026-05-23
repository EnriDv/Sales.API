using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class RestaurantOrderDetail
{
    public int Id { get; set; }

    public Guid Cen { get; set; }

    public int RestaurantOrderId { get; set; }

    public int ProductId { get; set; }

    public Guid ProductCen { get; set; }

    public int RestaurantOrderDetailStatusId { get; set; }

    public string? Note { get; set; }

    public int Quantity { get; set; }

    public DateTime? SentAt { get; set; }

    public int ResendCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual RestaurantOrder RestaurantOrder { get; set; } = null!;

    public virtual RestaurantOrderDetailStatus RestaurantOrderDetailStatus { get; set; } = null!;
}
