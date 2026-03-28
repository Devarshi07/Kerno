namespace NexusGrid.Shared.Exceptions;

public sealed class ValidationException : Exception
{
    public string ErrorCode { get; }
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IDictionary<string, string[]>? errors = null)
        : base(message)
    {
        ErrorCode = "VALIDATION_ERROR";
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}
