namespace UseCases.Results;

public record BackfillResult(int Created, int Skipped, int Failed)
{
    public static BackfillResult Empty => new(0, 0, 0);
}
