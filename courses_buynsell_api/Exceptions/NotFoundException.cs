// File: Exceptions/NotFoundException.cs
namespace courses_buynsell_api.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}