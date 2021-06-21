using System;
using System.Runtime.Serialization;

namespace Rsk.TokenExchange.Exceptions
{
    [Serializable]
    public class InvalidRequestException : TokenExchangeException
    {
        public InvalidRequestException() { }

        public InvalidRequestException(string message) : base(message) { }

        public InvalidRequestException(string message, Exception inner) : base(message, inner) { }

        protected InvalidRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}