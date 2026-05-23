using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class RestaurantOrderDetailStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<RestaurantOrderDetail> RestaurantOrderDetails { get; set; } = new List<RestaurantOrderDetail>();
}
