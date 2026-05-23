using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Shared.Core.Cen;
using Microsoft.EntityFrameworkCore;

namespace Sales.API.Application.Services;

public class WaiterService : IWaiterService
{
    private readonly IUnitOfWork _uow;

    public WaiterService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<WaiterContractResponse>> GetWaitersAsync(string companyCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var waiters = await _uow.Waiters.GetAllAsync();
        return waiters.Select(w => new WaiterContractResponse
        {
            WaiterCen = CenParser.Format(w.Cen),
            Name = w.Name
        }).ToList();
    }
}
