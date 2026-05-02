namespace UseCases.Exceptions;

/// <summary>
/// Thrown when a backfill entry should be skipped due to an expected domain condition,
/// such as a duplicate summary, no toll events found, or a missing vehicle.
/// </summary>
internal sealed class BackfillSkippedException : Exception
{
    public BackfillSkippedException(string message, Exception innerException)
        : base(message, innerException) { }
}
