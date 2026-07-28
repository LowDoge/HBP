namespace HBP.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Internal
}

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string message) => new("Validation", message, ErrorType.Validation);
    public static Error Conflict(string message) => new("Conflict", message, ErrorType.Conflict);
    public static Error Forbidden(string message) => new("Forbidden", message, ErrorType.Forbidden);
    public static Error Internal(string message) => new("Internal", message, ErrorType.Internal);

    public static Error NotFound(string entity, object id) =>
        new($"{entity}.NotFound", $"{entity} '{id}' not found", ErrorType.NotFound);
}
