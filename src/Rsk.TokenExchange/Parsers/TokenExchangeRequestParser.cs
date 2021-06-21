using System;
using System.Collections.Specialized;

namespace Rsk.TokenExchange
{
    public interface ITokenExchangeRequestParser
    {
        ITokenExchangeRequest Parse(string clientId, NameValueCollection parameters);
    }
    
    public class TokenExchangeRequestParser : ITokenExchangeRequestParser
    {
        public ITokenExchangeRequest Parse(string clientId, NameValueCollection parameters)
        {
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentNullException(nameof(clientId));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            return new TokenExchangeRequest(clientId, parameters);
        }
    }
}