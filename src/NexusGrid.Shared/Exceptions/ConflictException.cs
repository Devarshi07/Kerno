namespace NexusGrid.Shared.Exceptions;

public sealed class ConflictException : Exception
{
    public string ErrorCode { get; }

    public ConflictException(string message, string errorCode = "CONFLICT")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
