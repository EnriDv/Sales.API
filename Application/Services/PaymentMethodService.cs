using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;

namespace Sales.API.Application.Services;

public class PaymentMethodService : IPaymentMethodService
{
    public Task<List<PaymentMethodContractResponse>> GetPaymentMethodsAsync()
    {
        // En esta iteración, los métodos de pago son fijos.
        // Podrían salir de la base de datos más adelante si es necesario.
        return Task.FromResult(new List<PaymentMethodContractResponse>
        {
            new PaymentMethodContractResponse { PaymentMethodCen = "CASH", Name = "Efectivo", Description = "Pago en efectivo" },
            new PaymentMethodContractResponse { PaymentMethodCen = "QR", Name = "Pago con QR", Description = "Transferencia por código QR" },
            new PaymentMethodContractResponse { PaymentMethodCen = "CARD", Name = "Tarjeta", Description = "Pago con tarjeta de crédito o débito" }
        });
    }
}
