using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("company/{companyId}/location/{locationId}/active")]
    public async Task<IActionResult> GetActiveTickets(int companyId, int locationId) => 
        Ok(await _ticketService.GetActiveTicketsAsync(companyId, locationId));

    [HttpGet("company/{companyId}/location/{locationId}/history")]
    public async Task<IActionResult> GetTicketHistory(int companyId, int locationId, [FromQuery] int take = 100) =>
        Ok(await _ticketService.GetTicketHistoryAsync(companyId, locationId, take));

    [HttpGet("company/{companyId}/{ticketId}")]
    public async Task<IActionResult> GetTicket(int companyId, int ticketId) => 
        Ok(await _ticketService.GetTicketAsync(companyId, ticketId));

    [HttpPost("company/{companyId}")]
    public async Task<IActionResult> CreateTicket(int companyId, [FromBody] CreateTicketRequest request) => 
        Ok(await _ticketService.CreateTicketAsync(companyId, request));

    [HttpPost("company/{companyId}/{ticketId}/items")]
    public async Task<IActionResult> AddTicketItem(int companyId, int ticketId, [FromBody] AddTicketItemRequest request) => 
        Ok(await _ticketService.AddItemAsync(companyId, ticketId, request));

    [HttpPatch("company/{companyId}/{ticketId}/items/{itemId}/status")]
    public async Task<IActionResult> UpdateTicketItemStatus(int companyId, int ticketId, int itemId, [FromBody] UpdateTicketItemStatusRequest request) =>
        Ok(await _ticketService.UpdateItemStatusAsync(companyId, ticketId, itemId, request));

    [HttpPatch("company/{companyId}/{ticketId}/item/{itemId}/status")]
    public async Task<IActionResult> UpdateTicketItemStatusCompat(int companyId, int ticketId, int itemId, [FromBody] UpdateTicketItemStatusRequest request) =>
        Ok(await _ticketService.UpdateItemStatusAsync(companyId, ticketId, itemId, request));

    [HttpPost("company/{companyId}/{ticketId}/checkout")]
    public async Task<IActionResult> Checkout(int companyId, int ticketId, [FromBody] CheckoutRequest request) => 
        Ok(await _ticketService.CheckoutAsync(companyId, ticketId, request));

    [HttpDelete("company/{companyId}/{ticketId}")]
    public async Task<IActionResult> CancelTicket(int companyId, int ticketId)
    {
        await _ticketService.CancelTicketAsync(companyId, ticketId);
        return NoContent();
    }
}