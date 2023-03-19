using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api;

public partial class PrivateApiController
{
    [SwaggerResponse(200, "Operation is started")]
    [SwaggerResponse(400, "Invalid parameters")]
    [SwaggerOperation(
                Summary = "Start simulation of a number of generated orders",
                Description = "Start simulation of a number of generated orders",
                Tags = new[] { "Simulation" })]
    [HttpPost]
    public IActionResult PostSimulate([FromBody] PostSimulateRequest request)
    {
        this.simulationActorRef.Tell(new SimulationActor.StartOnceSimulation { Number = request.SimulateNumberOfOrders!.Value });
        return new OkResult();
    }

    [SwaggerResponse(200, "Operation is started")]
    [SwaggerResponse(400, "Invalid parameters")]
    [SwaggerOperation(
                Summary = "Start stress test",
                Description = "Start stress test",
                Tags = new[] { "Simulation" })]
    [HttpPost("stresstest")]
    public IActionResult PostStressTest([FromBody] PostStressRequest request)
    {
        this.simulationActorRef.Tell(new SimulationActor.StartStressSimuliation
        {
            Duration = TimeSpan.FromMinutes(request.DurationInMinutes!.Value),
            Interval = TimeSpan.FromMilliseconds(request.IntervalInMilliseconds!.Value)
        });
        return new OkResult();
    }
}
