using System.Collections.Generic;
using System.Security.Claims;

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
        string ParseSubject(IEnumerable<Claim> subjectClaims, ITokenExchangeRequest request);
        
        /// <summary>
        /// Parses the actor claim (act) from the token exchange request.
        /// </summary>
        /// <param name="subjectClaims">Claims parsed from the token exchange request.</param>
        /// <param name="request">The token exchange request.</param>
        Actor ParseActorClaim(IEnumerable<Claim> subjectClaims, ITokenExchangeRequest request);
        
        /// <summary>
        /// Parses the additional claims to use during token generation.
        /// </summary>
        /// <param name="subjectClaims">Claims parsed from the token exchange request.</param>
        /// <param name="request">The token exchange request.</param>
        IEnumerable<Claim> ParseAdditionalClaims(IEnumerable<Claim> subjectClaims, ITokenExchangeRequest request);
    }
}