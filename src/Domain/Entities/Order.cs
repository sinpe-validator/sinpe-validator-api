using System.Collections.Generic;

namespace sinpe_validator_api.Domain.Entities
{
    public class Order
    {
        public int IdOrder { get; set; }
        public decimal Amount { get; set; }
        public int IdStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? Description { get; set; }

        public string? OrderCode { get; set; }

        public OrderStatus? Status { get; set; }
        public ICollection<OrderPayment> Payments { get; set; } = new List<OrderPayment>();
    }
}
