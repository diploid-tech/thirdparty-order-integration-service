using Avanti.Core.RelationalData;

namespace Avanti.ThirdPartyOrderIntegrationService.Order.Migrations
{
    public class MigrationMarker : IMigrationMarker
    {
        public string Schema => "order";
    }
}
