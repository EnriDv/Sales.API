using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public int CompanyId { get; set; }

    public Guid CompanyCen { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
