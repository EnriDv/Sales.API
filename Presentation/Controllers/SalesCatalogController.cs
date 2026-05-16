using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/catalog")]
public class SalesCatalogController : ControllerBase
{
    private readonly ISalesCatalogService _catalog;

    public SalesCatalogController(ISalesCatalogService catalog) => _catalog = catalog;

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        string companyCen,
        [FromQuery] string? search = null,
        [FromQuery] string? categoryCen = null,
        [FromQuery] string? warehouseCen = null,
        [FromQuery] bool onlyAvailable = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50) =>
        Ok(await _catalog.GetSellableProductsAsync(companyCen, search, categoryCen, warehouseCen, onlyAvailable, page, pageSize));
}
