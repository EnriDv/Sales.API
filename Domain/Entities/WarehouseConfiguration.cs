using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class WarehouseConfiguration
{
    public int CompanyId { get; set; }

    public Guid CompanyCen { get; set; }

    public int MainWarehouseId { get; set; }

    public Guid MainWarehouseCen { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
