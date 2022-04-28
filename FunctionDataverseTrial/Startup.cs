using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

[assembly: FunctionsStartup(typeof(FunctionDataverseTrial.Startup))]

namespace FunctionDataverseTrial;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddScoped<IOrganizationService>(sp =>
        {
            var cs = sp.GetRequiredService<IConfiguration>().GetConnectionStringOrSetting("DefaultConnection");
            return new ServiceClient(cs);
        });
    }
}