using System;
using System.Runtime.Serialization;

namespace Rsk.TokenExchange.Exceptions
{
    public class SubjectParsingException : TokenExchangeException
    {
        public SubjectParsingException() { }

        public SubjectParsingException(string message) : base(message) { }

        public SubjectParsingException(string message, Exception inner) : base(message, inner) { }

        protected SubjectParsingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}