using System;

namespace ShortN.Exceptions;

public class CustomCodeNotAvailableException : Exception
{
    public CustomCodeNotAvailableException(string message) : base(message)
    {
    }

    public CustomCodeNotAvailableException() : base("The requested custom code is not available")
    {
    }
} 