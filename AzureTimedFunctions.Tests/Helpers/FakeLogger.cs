using Microsoft.Extensions.Logging;

namespace AzureTimedFunctions.Tests.Helpers;

/// <summary>
/// En in-memory logger som samlar loggposter för assertion i tester.
/// </summary>
public sealed class FakeLogger<T> : ILogger<T>
{
    private readonly List<FakeLogEntry> _entries = [];

    public IReadOnlyList<FakeLogEntry> Entries => _entries;

    public bool HasEntry(LogLevel level, string containsMessage) =>
        _entries.Any(e => e.Level == level && e.Message.Contains(containsMessage, StringComparison.OrdinalIgnoreCase));

    public int CountEntries(LogLevel level) =>
        _entries.Count(e => e.Level == level);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _entries.Add(new FakeLogEntry(logLevel, formatter(state, exception), exception));
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

public sealed record FakeLogEntry(LogLevel Level, string Message, Exception? Exception);
