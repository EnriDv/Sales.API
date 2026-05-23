using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/waiters")]
public class WaitersController : ControllerBase
{
    private readonly IWaiterService _waiters;

    public WaitersController(IWaiterService waiters) => _waiters = waiters;

    [HttpGet]
    public async Task<IActionResult> GetWaiters(string companyCen) =>
        Ok(await _waiters.GetWaitersAsync(companyCen));
}
