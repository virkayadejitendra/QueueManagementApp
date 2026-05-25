namespace QueueManagement.Api.Application.Exceptions;

public sealed class QueueEntryNotFoundException(string message) : Exception(message);
