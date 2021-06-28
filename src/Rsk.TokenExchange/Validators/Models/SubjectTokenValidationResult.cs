using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Rsk.TokenExchange.Validators
{
    /// <summary>
    /// The subject token validation result.
    /// </summary>
    public interface ISubjectTokenValidationResult
    {
        /// <summary>
        /// If the subject token is valid.
        /// </summary>
        bool IsValid { get; }
        
        /// <summary>
        /// The subject claims, parsed from the subject token.
        /// </summary>
        IEnumerable<Claim> Claims { get; }
    }

    /// <inheritdoc />
    public class SubjectTokenValidationResult : ISubjectTokenValidationResult
    {
        private SubjectTokenValidationResult() { }

        /// <summary>
        /// Creates a new success result.
        /// </summary>
        public static SubjectTokenValidationResult Success(IEnumerable<Claim> claims)
        {
            if (claims == null) throw new ArgumentNullException(nameof(claims));
            
            return new SubjectTokenValidationResult {IsValid = true, Claims = claims};
        }

        /// <summary>
        /// Creates a new failure result.
        /// </summary>
        public static SubjectTokenValidationResult Failure()
        {
            return new SubjectTokenValidationResult {IsValid = false};
        }
        
        /// <inheritdoc />
        public bool IsValid { get; private set; }
        
        /// <inheritdoc />
        public IEnumerable<Claim> Claims { get; private set; }
    }
}