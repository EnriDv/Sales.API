using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class TaxConfiguration
{
    public int CompanyId { get; set; }

    public Guid CompanyCen { get; set; }

    public decimal GlobalTaxPercentage { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
