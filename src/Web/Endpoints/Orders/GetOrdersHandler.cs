using Application.DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace sinpe_validator_api.Web.Endpoints.Orders;

public static class GetOrdersHandler
{
    public static async Task<IResult> Handle(SinpePaymentsDbContext dbContext)
    {
        var orders = await dbContext.Orders
            .Include(o => o.Status)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto
            {
                IdOrder = o.IdOrder,
                OrderCode = o.OrderCode ?? string.Empty,
                Amount = o.Amount,
                Status = o.Status != null ? o.Status.Name : "Unknown",
                CreatedAt = o.CreatedAt,
                ExpiresAt = o.ExpiresAt,
                Description = o.Description
            })
            .ToListAsync();

        return Results.Ok(orders);
    }
}
