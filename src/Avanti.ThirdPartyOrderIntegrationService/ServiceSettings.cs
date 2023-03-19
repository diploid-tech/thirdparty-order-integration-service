using Avanti.Core.Microservice.Settings;

namespace Avanti.ThirdPartyOrderIntegrationService;

public class ServiceSettings : IValidatable
{
    public Uri? OrderServiceUri { get; set; }
    public string ExternalSystemName { get; set; } = "unknown";
}
