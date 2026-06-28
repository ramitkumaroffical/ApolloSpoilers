namespace ApolloSpoilers.Domain.Exceptions;

/// <summary>Base type for all domain-level business rule violations.</summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string message, string errorCode = "DOMAIN_ERROR") : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, Exception inner, string errorCode = "DOMAIN_ERROR") : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} with key '{key}' was not found.", "NOT_FOUND") { }
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message, "CONFLICT") { }
}

public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", "VALIDATION")
    {
        Errors = new Dictionary<string, string[]>(errors);
    }
}
