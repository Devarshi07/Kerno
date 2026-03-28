namespace NexusGrid.Shared.Exceptions;

public sealed class NotFoundException : Exception
{
    public string ErrorCode { get; }

    public NotFoundException(string message, string errorCode = "NOT_FOUND")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public NotFoundException(string entityName, object id)
        : base($"{entityName} with ID '{id}' was not found.")
    {
        ErrorCode = $"{entityName.ToUpperInvariant()}_NOT_FOUND";
    }
}
