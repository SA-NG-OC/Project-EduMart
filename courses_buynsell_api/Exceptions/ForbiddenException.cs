// File: Exceptions/ForbiddenException.cs
namespace courses_buynsell_api.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
