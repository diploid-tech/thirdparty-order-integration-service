using System.Threading.Tasks;
using Akka.Actor;
using Avanti.Core.Microservice.Web;
using Avanti.ThirdPartyOrderIntegrationService.Order;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api
{
    public partial class PublicApiController
    {
        [SwaggerResponse(200, "The order is queued")]
        [SwaggerResponse(400, "The order is invalid")]
        [SwaggerOperation(
                    Summary = "Upsert an third party order",
                    Description = "Inserts the third party order to be processed",
                    Tags = new[] { "Order" })]
        [HttpPost]
        [BasicAuthorize]
        public async Task<IActionResult> PostOrder([FromBody] PostOrderRequest request) =>
            await orderActorRef.Ask<OrderActor.IResponse>(
                mapper.Map<OrderActor.InsertExternalOrder>(request)) switch
            {
                OrderActor.OrderReceived stored => new OkResult(),
                OrderActor.OrderAlreadyProcessed _ => new OkResult(),
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
            };
    }
}
