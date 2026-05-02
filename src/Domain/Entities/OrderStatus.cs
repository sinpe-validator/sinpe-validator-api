using System;
using System.Collections.Generic;
using System.Text;

namespace sinpe_validator_api.Domain.Entities;

public class OrderStatus
{
    public int IdStatus { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
