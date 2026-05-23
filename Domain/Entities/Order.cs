using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class Order
{
    public int Id { get; set; }

    public int DailyNumber { get; set; }

    public DateTime OrderDatetime { get; set; }

    public int OrderStatusId { get; set; }

    public int CustomerId { get; set; }

    public int CompanyId { get; set; }

    public Guid CompanyCen { get; set; }

    public decimal TaxPrice { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual OrderStatus OrderStatus { get; set; } = null!;

    public virtual ICollection<RestaurantOrder> RestaurantOrders { get; set; } = new List<RestaurantOrder>();
}
