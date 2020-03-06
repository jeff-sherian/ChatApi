using System;

namespace ChatApi.Models
{
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message)
            : base(message)
        {
        }
    }

    public class IdentityException : Exception
    {
    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message)
            : base(message)
        {
        }
    }

    public class ExceptionResponse
    {
        public int StatusCode { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
    }
}
