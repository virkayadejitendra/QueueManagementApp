namespace QueueManagement.Api.Application.Exceptions;

public sealed class QueueClosedException(string message) : Exception(message);
