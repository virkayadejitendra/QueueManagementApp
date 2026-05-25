namespace QueueManagement.Api.Domain.BusinessRules;

public static class QueueEntryStatuses
{
    public const string Waiting = "Waiting";
    public const string Called = "Called";
    public const string Served = "Served";
    public const string Skipped = "Skipped";
    public const string Cancelled = "Cancelled";
}
