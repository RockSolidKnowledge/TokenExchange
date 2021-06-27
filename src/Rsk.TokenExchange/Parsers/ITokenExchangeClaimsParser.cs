using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rsk.TokenExchange
{
    /// <summary>
    /// Parses claims from the token exchange request.
    /// </summary>
    public interface ITokenExchangeClaimsParser
    {
        /// <summary>
        /// Parses the subject claim (sub, e.g. user ID) from the token exchange request.
        /// </summary>
        /// <param name="subjectClaims">Claims parsed from the token exchange request.</param>
        /// <param name="request">The token exchange request.</param>
        Task<string> ParseSubject(IEnumerable<Claim> subjectClaims, ITokenExchangeRequest request);
        
        /// <summary>
        /// Parses the subject claims to use during token generation.
        /// Useful for actor claim generation.
        /// </summary>
        /// <param name="subjectClaims">Claims parsed from the token exchange request.</param>
        /// <param name="request">The token exchange request.</param>
        Task<IEnumerable<Claim>> ParseClaims(IEnumerable<Claim> subjectClaims, ITokenExchangeRequest request);
    }
}