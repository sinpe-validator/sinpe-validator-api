using System;
using System.Collections.Generic;
using System.Text;

namespace sinpe_validator_api.Domain.Entities;

public class PaymentStatus
{
    public int IdStatus { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<OrderPayment> Payments { get; set; } = new List<OrderPayment>();
}
