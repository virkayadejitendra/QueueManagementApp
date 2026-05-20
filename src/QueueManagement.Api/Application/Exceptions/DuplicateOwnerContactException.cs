namespace QueueManagement.Api.Application.Exceptions;

public sealed class DuplicateOwnerContactException(string message) : Exception(message);
