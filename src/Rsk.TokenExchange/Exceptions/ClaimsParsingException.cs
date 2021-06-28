using System;
using System.Runtime.Serialization;

namespace Rsk.TokenExchange.Exceptions
{
    /// <summary>
    /// Thrown when the default claims parser cannot understand the incoming subject.
    /// </summary>
    public class SubjectParsingException : TokenExchangeException
    {
        /// <inheritdoc />
        public SubjectParsingException() { }

        /// <inheritdoc />
        public SubjectParsingException(string message) : base(message) { }

        /// <inheritdoc />
        public SubjectParsingException(string message, Exception inner) : base(message, inner) { }

        /// <inheritdoc />
        protected SubjectParsingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}