using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorController : ControllerBase
{
    private readonly IBusinessService _businessService;
    public VendorController(IBusinessService businessService) => _businessService = businessService;

    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetVendors(int companyId) => Ok(await _businessService.GetVendorsAsync(companyId));

    [HttpPost("company/{companyId}")]
    public async Task<IActionResult> CreateVendor(int companyId, [FromBody] CreateVendorRequest request) => Ok(await _businessService.CreateVendorAsync(companyId, request));

    [HttpPut("company/{companyId}/{vendorId}")]
    public async Task<IActionResult> UpdateVendor(int companyId, int vendorId, [FromBody] CreateVendorRequest request)
    {
        await _businessService.UpdateVendorAsync(companyId, vendorId, request);
        return NoContent();
    }

    [HttpDelete("company/{companyId}/{vendorId}")]
    public async Task<IActionResult> DeleteVendor(int companyId, int vendorId)
    {
        await _businessService.DeleteVendorAsync(companyId, vendorId);
        return NoContent();
    }
}