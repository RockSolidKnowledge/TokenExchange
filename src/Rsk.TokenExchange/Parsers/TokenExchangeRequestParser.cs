using System;
using System.Collections.Specialized;

namespace Rsk.TokenExchange
{
    /// <summary>
    /// Parses the token exchange request parameters from an incoming request.
    /// </summary>
    public interface ITokenExchangeRequestParser
    {
        /// <summary>
        /// Parses a token exchange token exchange request from a NameValueCollection. 
        /// </summary>
        /// <param name="clientId">The ID of the client (client_id) that made the token exchange request.</param>
        /// <param name="parameters">The NameValueCollection containing the request parameters</param>
        ITokenExchangeRequest Parse(string clientId, NameValueCollection parameters);
    }

    /// <inheritdoc />
    public class TokenExchangeRequestParser : ITokenExchangeRequestParser
    {
        /// <inheritdoc />
        public ITokenExchangeRequest Parse(string clientId, NameValueCollection parameters)
        {
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentNullException(nameof(clientId));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            return new TokenExchangeRequest(clientId, parameters);
        }
    }
}