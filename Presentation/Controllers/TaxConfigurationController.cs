using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/tax-configuration")]
public class TaxConfigurationController : ControllerBase
{
    private readonly ITaxConfigurationService _tax;

    public TaxConfigurationController(ITaxConfigurationService tax) => _tax = tax;

    [HttpGet]
    public async Task<IActionResult> Get(string companyCen) =>
        Ok(await _tax.GetTaxConfigurationAsync(companyCen));

    [HttpPut]
    public async Task<IActionResult> Update(string companyCen, [FromBody] UpdateTaxConfigurationContractRequest request) =>
        Ok(await _tax.UpdateTaxConfigurationAsync(companyCen, request));
}
