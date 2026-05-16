using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Shared.Core.Exceptions;

namespace Sales.API.Application.Services;

public class WaiterService : IWaiterService
{
    private readonly IUnitOfWork _uow;

    public WaiterService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<WaiterContractResponse>> GetWaitersAsync(string companyCen)
    {
        if (!int.TryParse(companyCen, out var companyId))
            throw new ValidationException($"CEN de empresa inválido: {companyCen}");

        var company = await _uow.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new NotFoundException($"Empresa no encontrada: {companyCen}");

        var vendors = await _uow.Vendors.GetAllAsync(
            v => v.CompanyId == companyId && v.IsWaiter && v.Active);

        return vendors.Select(v => new WaiterContractResponse
        {
            WaiterCen = v.Id.ToString(),
            Name = v.Name
        }).ToList();
    }
}
