using System;

namespace HttpProcessor.Exceptions
{

    public class HttpProcessorUnauthorizedException : Exception
    {

        public HttpProcessorUnauthorizedException()
        {
        }

        public HttpProcessorUnauthorizedException(string message) : base(message)
        {
        }

        public HttpProcessorUnauthorizedException(string message, Exception innerException) : base(message,
            innerException)
        {
        }

    }
}
