using System;
using System.Runtime.Serialization;

namespace Rsk.TokenExchange.Exceptions
{
    [Serializable]
    public abstract class TokenExchangeException : Exception
    {
        public TokenExchangeException() { }

        public TokenExchangeException(string message) : base(message) { }

        public TokenExchangeException(string message, Exception inner) : base(message, inner) { }

        protected TokenExchangeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}