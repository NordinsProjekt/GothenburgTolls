using Entities.Tolls;

namespace Domain.Factories.Tests;

public class TollEventTests
{
    private static TollEvent CreateTollEvent() =>
        new(DateTimeOffset.UtcNow.AddMinutes(-10), "ZoneA", Guid.NewGuid());

    [Fact]
    public void AssignToDailyTollSummary_WithValidId_ShouldSetDailyTollSummaryId()
    {
        var tollEvent = CreateTollEvent();
        var summaryId = Guid.NewGuid();

        tollEvent.AssignToDailyTollSummary(summaryId);

        Assert.Equal(summaryId, tollEvent.DailyTollSummaryId);
    }

    [Fact]
    public void AssignToDailyTollSummary_WithEmptyGuid_ShouldThrowArgumentException()
    {
        var tollEvent = CreateTollEvent();

        Assert.Throws<ArgumentException>(() =>
            tollEvent.AssignToDailyTollSummary(Guid.Empty));
    }

    [Fact]
    public void AssignToDailyTollSummary_WhenAlreadyAssigned_ShouldThrowInvalidOperationException()
    {
        var tollEvent = CreateTollEvent();
        tollEvent.AssignToDailyTollSummary(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() =>
            tollEvent.AssignToDailyTollSummary(Guid.NewGuid()));
    }
}
