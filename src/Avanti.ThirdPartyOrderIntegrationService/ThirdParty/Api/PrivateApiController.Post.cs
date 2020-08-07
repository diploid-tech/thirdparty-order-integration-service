using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Avanti.Core.Microservice.Web;
using Avanti.ThirdPartyOrderIntegrationService.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api
{
    public partial class PrivateApiController
    {
        [SwaggerResponse(200, "The order is queued")]
        [SwaggerResponse(400, "The order is invalid")]
        [SwaggerOperation(
                    Summary = "Upsert an third party order",
                    Description = "Inserts the third party order to be processed",
                    Tags = new[] { "Order" })]
        [HttpPost]
        public IActionResult PostSimulate([FromBody] PostSimulateRequest request)
        {
            for (int idx = 0; idx < request.SimulateNumberOfOrders; idx++)
            {
                var externalOrder = new OrderActor.InsertExternalOrder
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderDate = this.clock.Now().AddDays(this.random.Next(0, 5) * -1),
                    Products = Enumerable.Range(1, this.random.Next(1, 10)).Select(idx =>
                        new
                        {
                            ProductId = this.random.Next(1, 100),
                            Amount = this.random.Next(1, 10)
                        })
                        .GroupBy(l => l.ProductId)
                        .Select(l => new OrderActor.InsertExternalOrder.Product
                        {
                            ProductId = l.Key,
                            Amount = l.Sum(p => p.Amount)
                        })
                };

                this.orderActorRef.Tell(externalOrder);
            }

            this.logger.LogInformation($"Done with simulating {request.SimulateNumberOfOrders} orders");
            return new OkResult();
        }
    }
}
