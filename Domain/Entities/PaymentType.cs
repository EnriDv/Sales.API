using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class PaymentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
