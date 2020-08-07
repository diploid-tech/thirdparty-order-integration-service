using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Avanti.Core.Microservice.Web;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api
{
    public partial class PrivateApiController
    {
        public class PostSimulateRequest
        {
            [Required]
            public int? SimulateNumberOfOrders { get; set; }
        }
    }
}
