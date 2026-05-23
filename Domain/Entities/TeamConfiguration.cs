using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class TeamConfiguration
{
    public int CompanyId { get; set; }

    public int CategoryId { get; set; }

    public int TeamId { get; set; }

    public Guid CompanyCen { get; set; }

    public Guid CategoryCen { get; set; }

    public virtual Team Team { get; set; } = null!;
}
