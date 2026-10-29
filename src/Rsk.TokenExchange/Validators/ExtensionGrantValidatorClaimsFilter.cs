namespace Rsk.TokenExchange;

using System;
using System.Collections.Generic;
using System.Security.Claims;

/// <summary>
/// Defines claim filtering rules used before creating an extension-grant validation result.
/// </summary>
public interface IExtensionGrantValidatorClaimsFilter
{
    /// <summary>
    /// Determines whether the claim should be included in the resulting subject principal.
    /// </summary>
    /// <param name="claim">A claim parsed from the subject token.</param>
    /// <returns><c>true</c> when the claim should be included; otherwise <c>false</c>.</returns>
    bool ShouldIncludeClaim(Claim claim);
}

/// <summary>
/// Default implementation that excludes identity claims populated by GrantValidationResult itself.
/// </summary>
public class ExtensionGrantValidatorClaimsFilter : IExtensionGrantValidatorClaimsFilter
{
    private static readonly List<string> DefaultExcludedClaimTypes =
    [
        "sub",
        "idp",
        "auth_time"
    ];

    /// <summary>
    /// Gets the claim types excluded from the resulting subject principal.
    /// Override to customize excluded claimtypes.
    /// </summary>
    protected virtual List<string> ExcludedClaimTypes => DefaultExcludedClaimTypes;

    /// <inheritdoc />
    public bool ShouldIncludeClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        return !ExcludedClaimTypes.Contains(claim.Type);
    }
}