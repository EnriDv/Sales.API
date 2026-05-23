using System;
using System.Collections.Generic;

namespace Sales.API.Domain.Entities;

public partial class Team
{
    public int Id { get; set; }

    public Guid Cen { get; set; }

    public string Name { get; set; } = null!;

    public int CompanyId { get; set; }

    public Guid CompanyCen { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<TeamConfiguration> TeamConfigurations { get; set; } = new List<TeamConfiguration>();
}
