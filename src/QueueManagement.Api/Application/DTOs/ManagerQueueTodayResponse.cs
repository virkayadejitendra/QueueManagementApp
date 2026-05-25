namespace QueueManagement.Api.Application.DTOs;

public sealed record ManagerQueueTodayResponse(
    int QueueLocationId,
    string LocationCode,
    string BusinessName,
    bool IsQueueOpen,
    int WaitingCount,
    ManagerQueueEntryResponse? CurrentCalled,
    IReadOnlyList<ManagerQueueEntryResponse> WaitingEntries,
    IReadOnlyList<ManagerQueueEntryResponse> SkippedEntries,
    IReadOnlyList<ManagerQueueEntryResponse> RecentServedEntries);
