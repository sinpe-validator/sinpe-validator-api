using System;
using System.Collections.Generic;
using System.Text;
using sinpe_validator_api.Domain.Entities;

namespace Application.Contracts;

public interface IOrderRepository
{
    Task<Order?> GetByCodeAndPendingStatusAsync(string orderCode);
}
