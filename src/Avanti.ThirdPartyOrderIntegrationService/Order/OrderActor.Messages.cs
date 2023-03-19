namespace Avanti.ThirdPartyOrderIntegrationService.Order;

public partial class OrderActor
{
    public class InsertExternalOrder
    {
        public string Id { get; set; } = "unknown";

        public DateTimeOffset OrderDate { get; set; }

        public IEnumerable<Product> Products { get; set; } = Array.Empty<Product>();

        public class Product
        {
            public int ProductId { get; set; }
            public int Amount { get; set; }
        }
    }
}
