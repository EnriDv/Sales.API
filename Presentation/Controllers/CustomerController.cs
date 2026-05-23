using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

/// <summary>
/// Endpoint INTERNO de administración. NO es parte del contrato público.
/// Los clientes NO están en el scope actual del contrato.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class CustomerController : ControllerBase
{
    private readonly IBusinessService _businessService;
    public CustomerController(IBusinessService businessService) => _businessService = businessService;

    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetCustomers(int companyId) => Ok(await _businessService.GetCustomersAsync(companyId));

    [HttpPost("company/{companyId}")]
    public async Task<IActionResult> CreateCustomer(int companyId, [FromBody] CreateCustomerRequest request) => Ok(await _businessService.CreateCustomerAsync(companyId, request));

    [HttpPut("company/{companyId}/{customerId}")]
    public async Task<IActionResult> UpdateCustomer(int companyId, int customerId, [FromBody] CreateCustomerRequest request)
    {
        await _businessService.UpdateCustomerAsync(companyId, customerId, request);
        return NoContent();
    }

    [HttpDelete("company/{companyId}/{customerId}")]
    public async Task<IActionResult> DeleteCustomer(int companyId, int customerId)
    {
        await _businessService.DeleteCustomerAsync(companyId, customerId);
        return NoContent();
    }
}