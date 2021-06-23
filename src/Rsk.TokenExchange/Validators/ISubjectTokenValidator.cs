using System.Threading.Tasks;

namespace Rsk.TokenExchange.Validators
{
    /// <summary>
    /// Validator for the subject token from a token exchange request
    /// </summary>
    public interface ISubjectTokenValidator
    {
        /// <summary>
        /// Validates the subject token
        /// </summary>
        /// <param name="token">the subject token (subject_token)</param>
        /// <param name="tokenType">the subject token type (subject_token_type)</param>
        /// <returns>Validation result</returns>
        Task<ISubjectTokenValidationResult> Validate(string token, string tokenType);
    }
}