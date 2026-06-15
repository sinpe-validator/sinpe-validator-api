using Application.DTOs;
using sinpe_validator_api.Domain.Entities;

namespace Application.Contracts;

public interface IPaymentRepository
{
    Task<List<UnmatchedPaymentDto>> GetUnmatchedAsync(int unmatchedStatusId);
    Task<OrderPayment?> GetByIdAsync(int idOrderPayment);
    Task<Order?> GetOrderByIdAsync(int idOrder);
    Task SaveChangesAsync();
}
