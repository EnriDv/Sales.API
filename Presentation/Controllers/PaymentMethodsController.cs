using Microsoft.AspNetCore.Mvc;
using Sales.API.Application.Interfaces;

namespace Sales.API.Presentation.Controllers;

[ApiController]
[Route("api/sales/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethods;

    public PaymentMethodsController(IPaymentMethodService paymentMethods) => _paymentMethods = paymentMethods;

    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await _paymentMethods.GetPaymentMethodsAsync());
}
