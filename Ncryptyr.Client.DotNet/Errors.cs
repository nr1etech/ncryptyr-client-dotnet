namespace Ncryptyr.Client.DotNet;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class InternalServerException : Exception
{
    public InternalServerException(string message) : base(message) { }
}
