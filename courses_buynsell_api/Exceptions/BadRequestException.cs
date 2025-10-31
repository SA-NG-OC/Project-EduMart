// File: Exceptions/BadRequestException.cs
namespace courses_buynsell_api.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
