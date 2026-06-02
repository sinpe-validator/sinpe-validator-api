using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Results;

namespace Application.Contracts;

public interface IOrderService
{
    Task<OrderServiceResult> ExpireOrder(int orderId);
}
