using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Rsk.TokenExchange.Validators
{
    public interface ISubjectTokenValidationResult
    {
        bool IsValid { get; }
        IEnumerable<Claim> Claims { get; }
    }
    
    public class SubjectTokenValidationResult : ISubjectTokenValidationResult
    {
        private SubjectTokenValidationResult() { }

        public static SubjectTokenValidationResult Success(IEnumerable<Claim> claims)
        {
            if (claims == null) throw new ArgumentNullException(nameof(claims));
            
            return new SubjectTokenValidationResult {IsValid = true, Claims = claims};
        }

        public static SubjectTokenValidationResult Failure()
        {
            return new SubjectTokenValidationResult {IsValid = false};
        }
        
        public bool IsValid { get; private set; }
        public IEnumerable<Claim> Claims { get; private set; }
    }
}