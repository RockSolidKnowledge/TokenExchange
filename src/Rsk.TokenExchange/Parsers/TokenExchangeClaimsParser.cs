using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using Rsk.TokenExchange.Exceptions;

namespace Rsk.TokenExchange
{
    /// <inheritdoc />
    public class TokenExchangeClaimsParser : ITokenExchangeClaimsParser
    {
        /// <inheritdoc />
        /// <returns>Returns the single subject value from the subject token claims.</returns>
        /// <exception cref="SubjectParsingException">Throws when more than one subject claim is present in the subject token claims.</exception>
        public virtual Task<string> ParseSubject(IEnumerable<Claim> subjectClaims, ITokenExchangeRequest request)
        {
            if (subjectClaims == null) throw new ArgumentNullException(nameof(subjectClaims));

            var subjectValues = subjectClaims.Where(x => x.Type == "sub").Select(x => x.Value).ToList();
            if (1 < subjectValues.Count) throw new SubjectParsingException("More than one subject claim present on token exchange request");

            return Task.FromResult(subjectValues.FirstOrDefault());
        }

        /// <inheritdoc />
        /// <returns>Returns the subject claims along with an actor claim when client_id != subject token client_id</returns>
        public virtual Task<IEnumerable<Claim>> ParseClaims(IEnumerable<Claim> subjectClaims, ITokenExchangeRequest request)
        {
            if (subjectClaims == null) throw new ArgumentNullException(nameof(subjectClaims));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var claims = subjectClaims.ToList();
            
            // add actor claim
            var actorClientId = claims.FirstOrDefault(x => x.Type == "client_id")?.Value;
            if (request.ClientId != actorClientId)
            {
                var previousActor = claims.FirstOrDefault(x => x.Type == "act");
                if (previousActor != null) claims.Remove(previousActor);
                
                var actor = new Actor {ClientId = actorClientId, InnerActor = previousActor?.Value};
                claims.Add(new Claim(
                    "act",
                    JsonConvert.SerializeObject(actor, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}),
                    JsonClaimValueTypes.Json));
            }

            return Task.FromResult((IEnumerable<Claim>) claims);
        }
    }
}