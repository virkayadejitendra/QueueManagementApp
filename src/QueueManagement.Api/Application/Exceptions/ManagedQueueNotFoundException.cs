namespace QueueManagement.Api.Application.Exceptions;

public sealed class ManagedQueueNotFoundException(string message) : Exception(message);
