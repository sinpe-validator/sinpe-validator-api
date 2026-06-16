namespace sinpe_validator_api.Web.Endpoints.Orders;

using Application.DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


public static class GetOrderByIdHandler
{
    public static async Task<IResult> Handle(int id, SinpePaymentsDbContext dbContext)
    {
        var order = await dbContext.Orders
            .Include(o => o.Status)
            .Where(o => o.IdOrder == id)
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
            .FirstOrDefaultAsync();

        return order is null
            ? Results.NotFound(new { message = $"Orden {id} no encontrada." })
            : Results.Ok(order);
    }
}
