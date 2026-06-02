using Application.DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace sinpe_validator_api.Web.Endpoints.Orders;

public static class GetOrdersHandler
{
    public static async Task<IResult> Handle(
        SinpePaymentsDbContext dbContext,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        string? estado,
        decimal? montoMin,
        decimal? montoMax)
    {
        var query = dbContext.Orders
            .Include(o => o.Status)
            .AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(o => o.CreatedAt >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(o => o.CreatedAt <= fechaHasta.Value.AddDays(1).AddTicks(-1));

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(o => o.Status != null && o.Status.Name == estado);

        if (montoMin.HasValue)
            query = query.Where(o => o.Amount >= montoMin.Value);

        if (montoMax.HasValue)
            query = query.Where(o => o.Amount <= montoMax.Value);

        var orders = await query
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
