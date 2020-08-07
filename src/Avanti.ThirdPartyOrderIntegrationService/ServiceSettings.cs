using System;
using Avanti.Core.Microservice.Settings;

namespace Avanti.ThirdPartyOrderIntegrationService
{
    public class ServiceSettings : Validatable
    {
        public Uri? OrderServiceUri { get; set; }
        public string ExternalSystemName { get; set; } = "unknown";
    }
}
