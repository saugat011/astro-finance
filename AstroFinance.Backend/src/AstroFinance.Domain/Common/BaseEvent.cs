namespace AstroFinance.Domain.Common;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class BaseEvent
{
    public DateTime Timestamp { get; private set; }

    protected BaseEvent()
    {
        Timestamp = DateTime.UtcNow;
    }
}