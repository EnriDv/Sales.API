using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class SalesSettingController : ControllerBase
{
    private readonly IBusinessService _businessService;
    public SalesSettingController(IBusinessService businessService) => _businessService = businessService;

    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetSettings(int companyId) => Ok(await _businessService.GetSettingsAsync(companyId));

    [HttpPut("company/{companyId}")]
    public async Task<IActionResult> UpdateSettings(int companyId, [FromBody] UpdateSalesSettingRequest request)
    {
        await _businessService.UpdateSettingsAsync(companyId, request);
        return NoContent();
    }
}