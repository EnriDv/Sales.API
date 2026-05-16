namespace Sales.API.Domain.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Location
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Category
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public decimal Price { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool Active { get; set; }
    public string? StationCode { get; set; }

    public virtual Category? Category { get; set; }
    public virtual ICollection<ProductStock> Stocks { get; set; } = new List<ProductStock>();
}

public class ProductStock
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public decimal Quantity { get; set; }

    public virtual Product Product { get; set; } = null!;
    public virtual WarehouseRef Warehouse { get; set; } = null!;
}

public class WarehouseRef
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
}