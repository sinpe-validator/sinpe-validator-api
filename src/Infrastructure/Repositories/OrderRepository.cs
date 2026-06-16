using Application.Contracts;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using sinpe_validator_api.Domain.Entities;

namespace Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private const int OrderStatusPending = 1;
    private readonly SinpePaymentsDbContext _dbContext;

    public OrderRepository(SinpePaymentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByCodeAndPendingStatusAsync(string orderCode)
    {
        return await _dbContext.Orders
            .FirstOrDefaultAsync<Order>(o => 
                o.OrderCode == orderCode && 
                o.IdStatus == OrderStatusPending);
    }

    public async Task<Order?> GetByCodeAsync(string orderCode)
    {
        return await _dbContext.Orders
            .FirstOrDefaultAsync<Order>(o => o.OrderCode == orderCode);
    }

    public async Task<bool> MarkAsExpiredAsync(int orderId, int expiredStatus)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.IdOrder == orderId);

        if (order is null)
            return false;

        order.IdStatus = expiredStatus;
        _dbContext.Orders.Update(order);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
