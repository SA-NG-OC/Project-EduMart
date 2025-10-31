// File: Exceptions/UnauthorizedException.cs
namespace courses_buynsell_api.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}