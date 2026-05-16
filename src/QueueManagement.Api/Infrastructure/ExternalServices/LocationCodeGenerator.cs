using System.Security.Cryptography;
using QueueManagement.Api.Application.Interfaces;

namespace QueueManagement.Api.Infrastructure.ExternalServices;

public sealed class LocationCodeGenerator : ILocationCodeGenerator
{
    private const string LocationCodeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public string Create()
    {
        Span<char> code = stackalloc char[8];

        for (var index = 0; index < code.Length; index++)
        {
            code[index] = LocationCodeCharacters[
                RandomNumberGenerator.GetInt32(LocationCodeCharacters.Length)];
        }

        return new string(code);
    }
}
