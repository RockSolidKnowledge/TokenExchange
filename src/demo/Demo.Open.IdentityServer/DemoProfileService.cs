using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;

namespace IdentityServer;

public class DemoProfileService(ITelemetryService telemetry, ILogger<DefaultProfileService> logger)
    : DefaultProfileService(telemetry, logger)
{
    public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        await base.GetProfileDataAsync(context);
        
        context.Subject.Claims.Where(x=>x.Type=="act").ToList().ForEach(x=>
            context.IssuedClaims.Add(new Claim(x.Type, x.Value, IdentityServerConstants.ClaimValueTypes.Json)));
    }
}