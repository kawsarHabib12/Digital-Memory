namespace DigitalMemoryMap.BLL.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public IDictionary<string, string[]>? Errors { get; }

    public AppException(string message, int statusCode = 400, IDictionary<string, string[]>? errors = null) 
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message = "Resource not found") 
        : base(message, 404) { }
}

public class ValidationException : AppException
{
    public ValidationException(string message, IDictionary<string, string[]>? errors = null) 
        : base(message, 400, errors) { }

    public ValidationException(string field, string error) 
        : base("Validation failed.", 400, new Dictionary<string, string[]> { { field, new[] { error } } }) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message = "Resource already exists") 
        : base(message, 409) { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Please log in") 
        : base(message, 401) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission") 
        : base(message, 403) { }
}

public class PayloadTooLargeException : AppException
{
    public PayloadTooLargeException(string message = "Payload too large") 
        : base(message, 413) { }
}
