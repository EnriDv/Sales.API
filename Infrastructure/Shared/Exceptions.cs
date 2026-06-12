namespace Sales.API.Shared.Exceptions;


public class DomainException : Exception
{
    public string Code { get; set; } = "BAD_REQUEST";
    
    public DomainException(string message, string? code = null) : base(message)
    {
        if (code != null) Code = code;
    }
}

public class NotFoundException : Exception
{
    public string Code { get; set; } = "NOT_FOUND";
    
    public NotFoundException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public string Code { get; set; } = "CONFLICT";
    
    public ConflictException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public string Code { get; set; } = "FORBIDDEN";
    
    public ForbiddenException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public string Code { get; set; } = "VALIDATION_ERROR";
    public Dictionary<string, string[]> Errors { get; set; } = new();
    
    public ValidationException(string message, Dictionary<string, string[]>? errors = null) : base(message)
    {
        if (errors != null) Errors = errors;
    }
}
