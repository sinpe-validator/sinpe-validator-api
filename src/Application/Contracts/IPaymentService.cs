using Application.Common.Results;
using Application.DTOs;

namespace Application.Contracts;

public interface IPaymentService
{
    Task<List<UnmatchedPaymentDto>> GetUnmatchedPayments();
    Task<PaymentMatchResult> MatchPaymentToOrder(int idOrderPayment, int idOrder);
}
