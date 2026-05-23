# Examen Parcial — Jose Enrique Diaz Velarde

## Sección 1 — Identificación

- **Nombre completo:** Jose Enrique Diaz Velarde
- **Pareja asignada para el sábado:** Rafael Andres Vargas
- **Repositorio de Inventario:** [[link](https://github.com/EnriDv/Inventario.API)] 
- **Repositorio de Ventas:** [[link](https://github.com/EnriDv/Sales.API)] (este mismo)
- **Contrato API acordado en grupo:** [link al archivo contrato-api.yaml en este repo]
- **URL del Swagger autogenerado** (cuando levantás el backend localmente): http://localhost:5068/swagger

## Sección 2 — Decisiones técnicas con snippets

### 2.1 Árbol de carpetas del backend de Ventas

Pegá la estructura de carpetas de tu proyecto de Ventas. Ejemplo:

```
Sales.API/
├── Application/
│   ├── DTOs/
│   ├── Extensions/
│   ├── Interfaces/
│   └── Services/
├── Domain/
│   └── Entities/
├── Infrastructure/
│   ├── Middleware/
│   ├── Persistence/
│   └── Repositories/
├── Presentation/
│   └── Controllers/
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

Organicé Sales.API en capas para separar responsabilidades: la lógica de aplicación, el dominio, la infraestructura y la exposición HTTP. Eso hace que el código sea más fácil de mantener, probar y modificar sin acoplar todo en una sola carpeta.

### 2.2 Flujo de "registrar una venta"

Pegá los snippets del código que se ejecuta cuando un usuario confirma una venta, en orden:

1. El endpoint que recibe el request (Controller).
```C#
[HttpPost("{ticketCen}/payment")]
    public async Task<IActionResult> PayTicket(string companyCen, string ticketCen, [FromBody] PayTicketContractRequest request)
    {
        var totals = await _ticketService.GetTicketTotalsAsync(companyCen, ticketCen);
        try
        {
            var result = await _ticketService.PayTicketAsync(companyCen, ticketCen, request);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            // Return 400 with validation details from inventory service
            var payload = new { message = ex.Message, errors = ex.Errors };
            return BadRequest(payload);
        }
        catch (ConflictException ex)
        {
            var response = new ProcessRestaurantOrderPaymentResultDto
            {
                IsSuccess = false,
                SaleCen = null,
                SaleId = null,
                InventoryDocumentCen = null,
                Subtotal = totals.Subtotal,
                TaxAmount = totals.TaxAmount,
                Total = totals.Total,
                Message = ex.Message,
                Insufficiencies = new List<StockInsufficiencyResponseDto>()
            };
            return Conflict(response);
        }
    }
```
2. La capa intermedia que procesa la lógica (Service / Use Case / Handler).
```C#
public async Task<PayTicketContractResponse> PayTicketAsync(string companyCen, string ticketCen, PayTicketContractRequest request)
    {
        var companyId = await SalesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var ticket = await SalesCenResolver.ResolveTicketAsync(_uow, companyId, ticketCen);
        var taxRate = await GetCompanyTaxRateAsync(companyId);
        var amounts = CalculateTicketAmounts(ticket, taxRate);
        var paymentType = (await _uow.PaymentTypes.GetAllAsync(p => p.Name.ToUpper() == request.PaymentMethodCode.ToUpper())).FirstOrDefault();
        var paymentTypeId = paymentType?.Id ?? 1;
        var companyCenGuid = CenParser.ParseRequired(companyCen, "empresa");
        await using var tx = await _ctx.Database.BeginTransactionAsync();
        try
        {
            var sale = new Sale
            {
                Cen = Guid.NewGuid(),
                CompanyId = companyId,
                CompanyCen = companyCenGuid,
                CustomerId = ticket.Order.CustomerId,
                PaymentTypeId = paymentTypeId,
                SubtotalPrice = amounts.Subtotal,
                TaxPrice = amounts.TaxAmount,
                SaleDatetime = DateTime.UtcNow,
                DiscountPercentage = 0,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _uow.Sales.AddAsync(sale);
            ticket.Order.OrderStatusId = await GetOrderStatusIdAsync("PAID");
            _uow.Orders.Update(ticket.Order);
            await _uow.SaveAsync();
            var mainWarehouse = await SalesCenResolver.ResolveMainWarehouseCenAsync(_uow, companyCen);
            var consumeRequest = new ConsumeStockContractRequest
            {
                WarehouseCen = mainWarehouse,
                Source = "SALES",
                ReferenceCen = CenParser.Format(sale.Cen),
                Reason = $"Pago ticket {CenParser.Format(ticket.Cen)}",
                Items = ticket.RestaurantOrderDetails
                    .Where(i => !i.IsDeleted)
                    .Select(i => new StockItemRequest
                    {
                        ProductCen = CenParser.Format(i.ProductCen),
                        Quantity = i.Quantity
                    })
                    .ToList()
            };
            await _inventoryApiClient.ConsumeStockAsync(CenParser.Format(ticket.Order.CompanyCen), consumeRequest);
            await tx.CommitAsync();
            return new PayTicketContractResponse
            {
                SaleCen = CenParser.Format(sale.Cen),
                TicketCen = CenParser.Format(ticket.Cen),
                Status = "PAID",
                Subtotal = amounts.Subtotal,
                TaxAmount = amounts.TaxAmount,
                Total = amounts.TotalAmount
            };
        }
        catch (Shared.Core.Exceptions.ConflictException)
        {
            await tx.RollbackAsync();
            throw;
        }
        catch (Shared.Core.Exceptions.ValidationException)
        {
            await tx.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            throw new Exception($"Error procesando pago: {ex.Message}", ex);
        }
    }
```
3. La parte que llama al Inventario del compañero (HttpClient o equivalente).
```C#
public async Task ConsumeStockAsync(string companyCen, ConsumeStockContractRequest request)
    {
        var path = $"api/inventory/companies/{Uri.EscapeDataString(companyCen)}/stock/consume";
        using var response = await _httpClient.PostAsJsonAsync(path, request, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowUpstreamExceptionAsync(response, "consuming stock");
        }
    }
```
4. La parte que persiste la venta en tu BD.

esto se hace a travez del GenericRepository.cs con las funciones:
```C#
public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
```
para agregarlo y la funcion 
```C#
public async Task SaveAsync() => await _context.SaveChangesAsync();
```
para gauardar los cambios. que se lave como 
```C#
wait _uow.SaveAsync(); 
```
en el flujo

Explicá en 3-5 líneas por qué dividiste así las responsabilidades.

El endpoint encargado de recibir el request se realizo asi para que quede conforme al contrato acordado, con los requests y responses adecuados. 
En el caso de la logica para pagar se realiza como una transaccion para que no se procesen unos items y otros no y si ocurre un fallo en medio todo deberia de cancelarse.
La parte que llama al inventario usa el HttpClient con de forma asincrona para que se quede esperando la respuesta correcta y no devuelva nada hasta que le llegue el response.
Y la parte en la que persiste la venta osea cuando se registra la venta en la BD se realiza con GenericRepository y UnitOfWork para facilitar y evitar errores al momento de insertar los datos.

### 2.3 Llamada al Inventario del compañero

Pegá el código exacto donde tu Ventas llama al API del Inventario del compañero.

pasa en 3 ocasiones:
```C#
{
public async Task ConsumeStockAsync(string companyCen, ConsumeStockContractRequest request)
    {
        var path = $"api/inventory/companies/{Uri.EscapeDataString(companyCen)}/stock/consume";
        using var response = await _httpClient.PostAsJsonAsync(path, request, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowUpstreamExceptionAsync(response, "consuming stock");
        }
    }
}
```
```C#
private static string BuildSellableProductsPath(
        string companyCen,
        string? search,
        string? categoryCen,
        string? warehouseCen,
        bool onlyAvailable)
    {
        var path = $"api/inventory/companies/{Uri.EscapeDataString(companyCen)}/sellable-products";
        var queryParameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            queryParameters.Add($"search={Uri.EscapeDataString(search)}");
        }
        if (!string.IsNullOrWhiteSpace(categoryCen))
        {
            queryParameters.Add($"categoryCen={Uri.EscapeDataString(categoryCen)}");
        }
        if (!string.IsNullOrWhiteSpace(warehouseCen))
        {
            queryParameters.Add($"warehouseCen={Uri.EscapeDataString(warehouseCen)}");
        }
        queryParameters.Add($"onlyAvailable={onlyAvailable.ToString().ToLowerInvariant()}");
        return $"{path}?{string.Join("&", queryParameters)}";
    }
```
```C#
private static string BuildProductLookupPath(string companyCen)
        => $"api/inventory/companies/{Uri.EscapeDataString(companyCen)}/products/lookup";
```


Respondé brevemente:
- ¿Qué pasa si el compañero responde con código 200 OK?
   
    Significa que el sistema esta ejecutandose correctamente, no deberia haber ningun error. 
- ¿Qué pasa si responde con 404 o 500?
    
    Significa que no se encuentra algun recurso o ultimamente su base de datos no funciona
- ¿Qué pasa si el compañero está caído (timeout)?
    
    Significa que no se pudo conectar correctamente a su modulo de compras, ya sea por que se cayo o por la conexion a internet

### 2.4 Configuración de la URL del compañero

Pegá:
- La línea relevante de tu `.env.example` o `appsettings.json`.
```json
"ServiceUrls": {
    "InventoryAPI": "http://10.80.114.190:5143/"
  }
```

- El código que lee esa configuración y la usa para construir la llamada HTTP.
```C#
private const string InventoryApiConfigKey = "ServiceUrls:InventoryAPI";
    public static IServiceCollection AddInventoryApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IInventoryApiClient, InventoryApiClient>(client =>
        {
            var inventoryApiBaseUrl = configuration[InventoryApiConfigKey]
                ?? throw new InvalidOperationException($"Missing configuration value '{InventoryApiConfigKey}'.");
            client.BaseAddress = new Uri(inventoryApiBaseUrl.TrimEnd('/') + "/");
        });
        return services;
    }
```
Explicá en 1 línea cómo cambiarías esa URL si el sábado tu pareja levanta su backend en otra IP.

En este caso como solo esta configurado en el ServiceUrls solo se deberia cambiar ahi la referencia del inventario. 

## Sección 3 — Sobre el trabajo en grupo del contrato API

- **3.1** ¿Hubo desacuerdos al definir el contrato? ¿Cuáles?

Como tal no hubo desacuerdos, se llevo un buen dialogo entre los involucrados para seguir buenas practicas.

- **3.2** ¿Cómo se resolvieron?

Solo con dialogo mencionando sus ideas y como se harian hasta que se llgo a un acuerdo entre contratos realizado por Rafael.

- **3.3** ¿Qué propusiste vos específicamente que quedó en el contrato final?

Personalmente creo que ninguna jasjasj, todos tenian sus propios metodos y muchos de ellos ya se repetian asi que algo especifico que destacar no creo que haya entrado.

## Sección 4 — Teoría aplicada

Respondé cada pregunta en 1-2 párrafos. Está permitido usar IA para mejorar redacción, pero las respuestas deben hacer referencia explícita a tu propio código o decisiones.

**4.1** Tu compañero te avisa que va a cambiar el campo `cantidad` por `qty` en su respuesta del endpoint de stock. Tu sistema ya consume ese endpoint. Explicá qué riesgos genera ese cambio y qué prácticas conocés para evitar que un cambio así rompa los sistemas que dependen de su API.

Si el endpoint del Inventario cambia el nombre del campo cantidad a qty, los sistemas que lo consumen actuales dejarán de recibir el valor y fallarán ya sea con comportamiento incorrecto o directamente no funcionando.
En nuestro código la comunicación con Inventario está tipada y validada por contratos, y InventoryApiClient espera campos concretos.
Para evitarlo se puede mantener compatibilidad, versionar la API.
También se puede introducir un wrapper/adapter en el cliente que traduzca qty→cantidad si es necesario temporalmente.

---

**4.2** Tu sistema de Ventas hace una petición al Inventario para descontar stock. La red se cae justo después de que Inventario procesó el descuento pero antes de que la respuesta llegue a Ventas. ¿Qué problema se genera? ¿Cómo lo manejarías?

Ese escenario genera una condición de incertidumbre. La misma operación puede reintentarse provocando doble descuento, o la venta quedará registrada sin el stock correctamente descontado.
En mi flujo TicketService construye ReferenceCen = CenParser.Format(sale.Cen) antes de llamar a Inventario, precisamente para permitir idempotencia en Inventario.
La solución práctica es exigir que la API de Inventario sea idempotente por ReferenceCen y que ambos lados implementen reintentos seguros o un mecanismo de confirmación que ayuda si la operacion queda a medias.

---
**4.3** Si el Inventario del compañero está caído, ¿debería tu Ventas permitir seguir registrando ventas? Justificá considerando ventajas y desventajas de cada postura. ¿Qué hace TU sistema hoy en ese caso?

Permitir ventas offline mejora disponibilidad y ventas inmediatas, pero complica la consistencia de stock. Bloquear ventas mantiene integridad de inventario pero reduce disponibilidad y vende menos cuando la dependencia está caída.
En mi diseño actual se maneja dentro de una transacción y revierte si falla, por lo que hoy la API no permite completar el pago si Inventario falla (rollback) priorizamos consistencia sobre disponibilidad.

---
**4.4** Explicá por qué tener la URL del compañero hardcodeada como `http://localhost:5000` es un problema. ¿Cuál es la solución correcta y cómo la implementaste vos?

Hardcodear la URL impide desplegar en entornos diferentes y obliga a cambiar código para cada entorno, lo que es frágil y propenso a errores. La solución correcta es configurar la URL por entorno y leerla en tiempo de arranque.
En este modulo la configuramos en appsettings.json bajo ServiceUrls:InventoryAPI y la usamos al registrar el cliente HTTP en AddInventoryApiClient. Para cambiar la IP basta actualizar ServiceUrls:InventoryAPI en el appsettings.