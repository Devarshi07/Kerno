namespace NexusGrid.Shared.Models;

public sealed record ErrorResponse(
    string Error,
    string Code,
    object? Details = null
);
