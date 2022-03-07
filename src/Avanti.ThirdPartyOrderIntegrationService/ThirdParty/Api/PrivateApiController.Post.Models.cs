using System.ComponentModel.DataAnnotations;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api
{
    public partial class PrivateApiController
    {
        public class PostSimulateRequest
        {
            [Required]
            public int? SimulateNumberOfOrders { get; set; }
        }

        public class PostStressRequest
        {
            [Required]
            public int? DurationInMinutes { get; set; }

            [Required]
            public int? IntervalInMilliseconds { get; set; }
        }
    }
}
