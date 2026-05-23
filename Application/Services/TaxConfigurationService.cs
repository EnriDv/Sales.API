using Sales.API.Application.DTOs;
using Sales.API.Application.Interfaces;
using Shared.Core.Cen;

namespace Sales.API.Application.Services;

public class TaxConfigurationService : ITaxConfigurationService
{
    private readonly IUnitOfWork _uow;

    public TaxConfigurationService(IUnitOfWork uow) => _uow = uow;

    public async Task<TaxConfigurationContractResponse> GetTaxConfigurationAsync(string companyCen)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var config = (await _uow.TaxConfigurations.GetAllAsync(t => t.CompanyId == companyId)).FirstOrDefault();

        return new TaxConfigurationContractResponse
        {
            CompanyCen = companyCen,
            GlobalTaxPercentage = config?.GlobalTaxPercentage ?? 0m
        };
    }

    public async Task<TaxConfigurationContractResponse> UpdateTaxConfigurationAsync(string companyCen, UpdateTaxConfigurationContractRequest request)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var config = (await _uow.TaxConfigurations.GetAllAsync(t => t.CompanyId == companyId)).FirstOrDefault();

        if (config == null)
        {
            var cen = CenParser.ParseRequired(companyCen, "empresa");
            config = new Domain.Entities.TaxConfiguration
            {
                CompanyId = companyId,
                CompanyCen = cen,
                GlobalTaxPercentage = request.GlobalTaxPercentage,
                CreatedAt = DateTime.UtcNow
            };
            await _uow.TaxConfigurations.AddAsync(config);
        }
        else
        {
            config.GlobalTaxPercentage = request.GlobalTaxPercentage;
            _uow.TaxConfigurations.Update(config);
        }

        await _uow.SaveAsync();

        return new TaxConfigurationContractResponse
        {
            CompanyCen = companyCen,
            GlobalTaxPercentage = config.GlobalTaxPercentage
        };
    }
}
