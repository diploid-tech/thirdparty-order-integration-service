using System.CodeDom.Compiler;
using Avanti.Core.Microservice;
using Microsoft.Extensions.Configuration;

namespace Avanti.ThirdPartyOrderIntegrationService
{
    [GeneratedCode("avanti-cli", "1.0.0-beta+c101678abd")]
    public class Startup : StartupCore<ServiceSettings>
    {
        public Startup(IConfiguration config)
            : base(config)
        { }
    }
}
