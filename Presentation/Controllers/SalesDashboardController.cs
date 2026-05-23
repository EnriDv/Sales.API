using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/dashboard")]
public class SalesDashboardController : ControllerBase
{
    private readonly ISalesDashboardService _dashboard;

    public SalesDashboardController(ISalesDashboardService dashboard) => _dashboard = dashboard;

    [HttpGet("daily-sales")]
    public async Task<IActionResult> GetDailySales(string companyCen, [FromQuery] DateTime? date = null) =>
        Ok(await _dashboard.GetDailySalesAsync(companyCen, date));

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts(string companyCen, [FromQuery] int topN = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null) =>
        Ok(await _dashboard.GetTopProductsAsync(companyCen, topN, startDate, endDate));

    [HttpGet("kds-status")]
    public async Task<IActionResult> GetKdsStatus(string companyCen) =>
        Ok(await _dashboard.GetKdsStatusAsync(companyCen));
}
