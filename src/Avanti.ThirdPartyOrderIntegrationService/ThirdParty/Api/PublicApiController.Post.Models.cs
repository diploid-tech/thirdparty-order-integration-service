using System.ComponentModel.DataAnnotations;
using Avanti.Core.Microservice.Web;

namespace Avanti.ThirdPartyOrderIntegrationService.ThirdParty.Api;

public partial class PublicApiController
{
    public class PostOrderRequest
    {
        [Required]
        public string Id { get; set; } = "unknown";

        [Required]
        public DateTimeOffset OrderDate { get; set; }

        [Required]
        [MustHaveElements]
        public IEnumerable<Product> Products { get; set; } = Array.Empty<Product>();

        public class Product
        {
            public int ProductId { get; set; }
            public int Amount { get; set; }
        }
    }
}
