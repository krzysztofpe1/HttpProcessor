using System;

namespace HttpProcessor.Exceptions
{

    internal class HttpProcessorGeneralException : Exception
    {

        public HttpProcessorGeneralException()
        {
        }

        public HttpProcessorGeneralException(string message) : base(message)
        {
        }

        public HttpProcessorGeneralException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}