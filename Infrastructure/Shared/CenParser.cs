using Sales.API.Shared.Exceptions;

namespace Sales.API.Shared.Cen;


public static class CenParser
{
    public static Guid ParseRequired(string? value, string entityLabel = "entidad")
    {
        if (!TryParse(value, out var guid))
            throw new ValidationException($"CEN de {entityLabel} inválido: {value}");
        return guid;
    }

    public static bool TryParse(string? value, out Guid guid)
    {
        guid = Guid.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;
        return Guid.TryParse(value.Trim(), out guid) && guid != Guid.Empty;
    }

    public static string Format(Guid cen) => cen.ToString();
}
