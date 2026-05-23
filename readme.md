# Sales.API — README


## Configuración
- Archivo: `appsettings.json`
- Valores importantes:
  - `ConnectionStrings:DefaultConnection` — cadena Postgres
  - `ServiceUrls:InventoryAPI` — URL base del Inventario


## Base de datos (rápido)
1. Crear la base:

```
dotnet ef database update --project Sales.API
```

## Compilar y ejecutar
```bash
cd Sales.API
dotnet restore
dotnet build
dotnet run
```

Swagger: `http://localhost:5068/swagger`



