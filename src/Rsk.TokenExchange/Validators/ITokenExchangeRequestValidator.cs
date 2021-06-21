using System.Threading.Tasks;

namespace Rsk.TokenExchange.Validators
{
    /// <summary>
    /// Validates token exchange requests.
    /// This is where your custom authorization logic lives.
    /// </summary>
    public interface ITokenExchangeRequestValidator
    {
        /// <summary>
        /// Validates the token exchange request and extracts the subject.
        /// </summary>
        /// <param name="request">The token exchange request</param>
        /// <returns>The token exchange validation result</returns>
        Task<ITokenExchangeValidationResult> Validate(ITokenExchangeRequest request);
    }
}