using Application.Contracts;
using Application.DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using sinpe_validator_api.Domain.Entities;

namespace Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly SinpePaymentsDbContext _dbContext;

    public PaymentRepository(SinpePaymentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UnmatchedPaymentDto>> GetUnmatchedAsync(int unmatchedStatusId)
    {
        return await _dbContext.OrderPayments
            .Where(payment => payment.IdStatus == unmatchedStatusId)
            .OrderByDescending(payment => payment.ProcessedAt)
            .Select(payment => new UnmatchedPaymentDto
            {
                IdOrderPayment = payment.IdOrderPayment,

                IdSms = payment.IdSms,
                SenderName = payment.Sms != null ? payment.Sms.SenderName : null,
                Amount = payment.Sms != null ? payment.Sms.Amount : null,
                SinpeReference = payment.Sms != null ? payment.Sms.SinpeReference : null,
                SmsDescription = payment.Sms != null ? payment.Sms.Description : null,
                ReceivedAt = payment.Sms != null ? payment.Sms.ReceivedAt : null,
                RegisteredAt = payment.Sms != null ? payment.Sms.RegisteredAt : null,

                PaymentStatusId = payment.IdStatus,
                PaymentStatusName = payment.Status != null ? payment.Status.Name : null,
                RejectionReason = payment.RejectionReason,
                ProcessedAt = payment.ProcessedAt
            })
            .ToListAsync();
    }

    public async Task<OrderPayment?> GetByIdAsync(int idOrderPayment)
    {
        return await _dbContext.OrderPayments
            .FirstOrDefaultAsync(payment => payment.IdOrderPayment == idOrderPayment);
    }

    public async Task<Order?> GetOrderByIdAsync(int idOrder)
    {
        return await _dbContext.Orders
            .FirstOrDefaultAsync(order => order.IdOrder == idOrder);
    }

    public Task SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}
