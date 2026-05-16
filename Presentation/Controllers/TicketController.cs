using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

/// <summary>
/// VentasTicketsContract — Parte del contrato público v1
/// Base: /api/sales/companies/{companyCen}
/// </summary>
[ApiController]
[Route("api/sales/companies/{companyCen}/tickets")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets(string companyCen, [FromQuery] string? status = null) =>
        Ok(await _ticketService.GetTicketsAsync(companyCen, status));

    [HttpGet("{ticketCen}")]
    public async Task<IActionResult> GetTicket(string companyCen, string ticketCen) =>
        Ok(await _ticketService.GetTicketAsync(companyCen, ticketCen));

    [HttpPost]
    public async Task<IActionResult> CreateTicket(string companyCen, [FromBody] CreateTicketContractRequest request)
    {
        var result = await _ticketService.CreateTicketAsync(companyCen, request);
        return CreatedAtAction(nameof(GetTicket), new { companyCen, ticketCen = result.TicketCen }, result);
    }

    [HttpGet("{ticketCen}/totals")]
    public async Task<IActionResult> GetTicketTotals(string companyCen, string ticketCen) =>
        Ok(await _ticketService.GetTicketTotalsAsync(companyCen, ticketCen));

    [HttpPut("{ticketCen}/waiter")]
    public async Task<IActionResult> AssignWaiter(string companyCen, string ticketCen, [FromBody] AssignWaiterContractRequest request) =>
        Ok(await _ticketService.AssignWaiterAsync(companyCen, ticketCen, request));

    [HttpPost("{ticketCen}/send")]
    public async Task<IActionResult> SendToKitchen(string companyCen, string ticketCen)
    {
        await _ticketService.SendToKitchenAsync(companyCen, ticketCen);
        return Ok(new { message = "Ticket enviado a cocina exitosamente." });
    }

    [HttpPost("{ticketCen}/payment")]
    public async Task<IActionResult> PayTicket(string companyCen, string ticketCen, [FromBody] PayTicketContractRequest request) =>
        Ok(await _ticketService.PayTicketAsync(companyCen, ticketCen, request));

    [HttpPost("{ticketCen}/cancel")]
    public async Task<IActionResult> CancelTicket(string companyCen, string ticketCen, [FromBody] CancelTicketContractRequest? request = null) =>
        Ok(await _ticketService.CancelTicketAsync(companyCen, ticketCen, request));

    [HttpPost("{ticketCen}/print")]
    public async Task<IActionResult> PrintTicket(string companyCen, string ticketCen)
    {
        await _ticketService.PrintTicketAsync(companyCen, ticketCen);
        return Ok(new { message = "Ticket enviado a impresión exitosamente." });
    }

    // ── ITEMS ───────────────────────────────────────

    [HttpGet("{ticketCen}/items")]
    public async Task<IActionResult> GetTicketItems(string companyCen, string ticketCen) =>
        Ok(await _ticketService.GetTicketItemsAsync(companyCen, ticketCen));

    [HttpPost("{ticketCen}/items")]
    public async Task<IActionResult> AddTicketItem(string companyCen, string ticketCen, [FromBody] AddTicketItemContractRequest request) =>
        Ok(await _ticketService.AddItemAsync(companyCen, ticketCen, request));

    [HttpPatch("{ticketCen}/items/{ticketItemCen}")]
    public async Task<IActionResult> UpdateTicketItem(string companyCen, string ticketCen, string ticketItemCen, [FromBody] UpdateTicketItemContractRequest request) =>
        Ok(await _ticketService.UpdateItemAsync(companyCen, ticketCen, ticketItemCen, request));

    [HttpPost("{ticketCen}/items/{ticketItemCen}/resend")]
    public async Task<IActionResult> ResendTicketItem(string companyCen, string ticketCen, string ticketItemCen)
    {
        await _ticketService.ResendItemAsync(companyCen, ticketCen, ticketItemCen);
        return Ok(new { message = "Item reenviado exitosamente." });
    }
}