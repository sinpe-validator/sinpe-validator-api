using System;
using System.Collections.Generic;
using System.Text;

namespace sinpe_validator_api.Domain.Entities;

public class ReceivedSms
{
    public int IdSms { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string SinpeReference { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime RegisteredAt { get; set; }

    public ICollection<OrderPayment> Payments { get; set; } = new List<OrderPayment>();
}
