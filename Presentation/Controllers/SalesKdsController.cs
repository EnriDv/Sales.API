using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/kds")]
public class SalesKdsController : ControllerBase
{
    private readonly IKdsService _kds;

    public SalesKdsController(IKdsService kds) => _kds = kds;

    [HttpGet("teams")]
    public async Task<IActionResult> GetTeams(string companyCen) =>
        Ok(await _kds.GetTeamsAsync(companyCen));

    [HttpGet("teams/{teamCen}/items")]
    public async Task<IActionResult> GetTeamItems(string companyCen, string teamCen) =>
        Ok(await _kds.GetTeamItemsAsync(companyCen, teamCen));

    [HttpPatch("items/{ticketItemCen}/status")]
    public async Task<IActionResult> UpdateItemStatus(
        string companyCen,
        string ticketItemCen,
        [FromBody] UpdateKdsItemStatusContractRequest request)
    {
        await _kds.UpdateItemStatusAsync(companyCen, ticketItemCen, request);
        return NoContent();
    }
}
