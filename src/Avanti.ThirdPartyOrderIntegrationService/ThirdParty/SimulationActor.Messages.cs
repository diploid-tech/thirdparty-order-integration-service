namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty;

public partial class SimulationActor
{
    public class StartOnceSimulation
    {
        public int Number { get; set; }
    }

    public class StartStressSimuliation
    {
        public TimeSpan Duration { get; set; }
        public TimeSpan Interval { get; set; }
    }

    public class SendOrder
    { }
}
