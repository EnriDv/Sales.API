using System.ComponentModel.DataAnnotations;

namespace Sales.API.Application.DTOs;

public class ConsumeStockContractRequest
{
    public string? WarehouseCen { get; set; }
    public string? Source { get; set; }
    public string? ReferenceCen { get; set; }
    public string? Reason { get; set; }
    [Required, MinLength(1)] public List<StockItemRequest> Items { get; set; } = new();

    public string? Reference => ReferenceCen;
}

public class StockItemRequest
{
    [Required] public string ProductCen { get; set; } = string.Empty;
    [Required] public decimal Quantity { get; set; }
    public string? WarehouseCen { get; set; }
}
