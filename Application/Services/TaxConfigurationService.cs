using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Sales.API.Domain.Entities;
using Shared.Core.Exceptions;

namespace Sales.API.Application.Services;

public class TaxConfigurationService : ITaxConfigurationService
{
    private readonly IUnitOfWork _uow;

    public TaxConfigurationService(IUnitOfWork uow) => _uow = uow;

    private async Task<int> ResolveCompanyIdAsync(string companyCen)
    {
        if (!int.TryParse(companyCen, out var id))
            throw new ValidationException($"CEN de empresa inválido: {companyCen}");
        var company = await _uow.Companies.GetByIdAsync(id);
        if (company == null)
            throw new NotFoundException($"Empresa no encontrada: {companyCen}");
        return id;
    }

    public async Task<TaxConfigurationContractResponse> GetTaxConfigurationAsync(string companyCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var settings = await _uow.SalesSettings.GetAllAsync(s => s.CompanyId == companyId);
        var setting = settings.FirstOrDefault();

        return new TaxConfigurationContractResponse
        {
            CompanyCen = companyCen,
            GlobalTaxPercentage = setting?.TaxRate ?? 0
        };
    }

    public async Task<TaxConfigurationContractResponse> UpdateTaxConfigurationAsync(string companyCen, UpdateTaxConfigurationContractRequest request)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var settings = await _uow.SalesSettings.GetAllAsync(s => s.CompanyId == companyId);
        var setting = settings.FirstOrDefault();

        if (setting == null)
        {
            setting = new SalesSetting
            {
                CompanyId = companyId,
                TaxRate = request.GlobalTaxPercentage,
                PaymentMethods = "CASH,QR,CARD",
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _uow.SalesSettings.AddAsync(setting);
        }
        else
        {
            setting.TaxRate = request.GlobalTaxPercentage;
            setting.UpdatedAt = DateTime.UtcNow;
            _uow.SalesSettings.Update(setting);
        }

        await _uow.SaveAsync();

        return await GetTaxConfigurationAsync(companyCen);
    }
}
