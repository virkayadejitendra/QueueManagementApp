namespace QueueManagement.Api.Application.Exceptions;

public sealed class QueueConflictException(string message) : Exception(message);
