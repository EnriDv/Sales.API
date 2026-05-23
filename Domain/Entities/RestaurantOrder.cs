using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class RestaurantOrder
{
    public int Id { get; set; }

    public Guid Cen { get; set; }

    public int OrderId { get; set; }

    public int WaiterId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<RestaurantOrderDetail> RestaurantOrderDetails { get; set; } = new List<RestaurantOrderDetail>();

    public virtual Waiter Waiter { get; set; } = null!;
}
