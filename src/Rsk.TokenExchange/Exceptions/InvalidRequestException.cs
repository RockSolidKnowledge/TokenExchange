using System;
using System.Runtime.Serialization;

namespace Rsk.TokenExchange.Exceptions
{
    /// <summary>
    /// Thrown when a token exchange request cannot be parsed.
    /// </summary>
    [Serializable]
    public class InvalidRequestException : TokenExchangeException
    {
        /// <inheritdoc />
        public InvalidRequestException() { }

        /// <inheritdoc />
        public InvalidRequestException(string message) : base(message) { }

        /// <inheritdoc />
        public InvalidRequestException(string message, Exception inner) : base(message, inner) { }

        /// <inheritdoc />
        protected InvalidRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}