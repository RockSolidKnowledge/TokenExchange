using System;
using System.Runtime.Serialization;

namespace Rsk.TokenExchange.Exceptions
{
    /// <summary>
    /// Base class for custom token exchange exceptions.
    /// </summary>
    [Serializable]
    public abstract class TokenExchangeException : Exception
    {
        /// <inheritdoc />
        public TokenExchangeException() { }

        /// <inheritdoc />
        public TokenExchangeException(string message) : base(message) { }

        /// <inheritdoc />
        public TokenExchangeException(string message, Exception inner) : base(message, inner) { }

        /// <inheritdoc />
        protected TokenExchangeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}