namespace Sales.API.Shared.Responses;

// Estructura estándar para devolver errores limpios al Frontend
public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Opcional: Wrapper para respuestas exitosas estandarizadas
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa") 
        => new() { Success = true, Message = message, Data = data };
        
    public static ApiResponse<T> Fail(string message) 
        => new() { Success = false, Message = message, Data = default };
}
