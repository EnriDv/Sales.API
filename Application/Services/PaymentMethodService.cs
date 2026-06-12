using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Sales.API.Shared.Cen;

namespace Sales.API.Application.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IUnitOfWork _uow;

    public PaymentMethodService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<PaymentMethodContractResponse>> GetPaymentMethodsAsync()
    {
        var methods = await _uow.PaymentTypes.GetAllAsync();
        return methods.Select(m => new PaymentMethodContractResponse
        {
            PaymentMethodCode = m.Name,
            Name = m.Name,
            IsActive = true
        }).ToList();
    }
}
