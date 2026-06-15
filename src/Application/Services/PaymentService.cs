using Application.Common.Results;
using Application.Contracts;
using Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class PaymentService : IPaymentService
{
    private const int OrderPendingStatusId = 1;
    private const int OrderPaidStatusId = 2;
    private const int OrderExpiredStatusId = 3;
    private const int OrderUnderReviewStatusId = 4;

    private const int PaymentApprovedStatusId = 1;
    private const int PaymentUnmatchedStatusId = 4;

    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentRepository paymentRepository, ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public Task<List<UnmatchedPaymentDto>> GetUnmatchedPayments()
    {
        return _paymentRepository.GetUnmatchedAsync(PaymentUnmatchedStatusId);
    }

    public async Task<PaymentMatchResult> MatchPaymentToOrder(int idOrderPayment, int idOrder)
    {
        if (idOrder <= 0)
        {
            return new PaymentMatchResult
            {
                IsSuccess = false,
                Error = PaymentOperationError.Validation,
                ErrorMessage = "Debe indicar la orden a la que se asociará el pago."
            };
        }

        try
        {
            var payment = await _paymentRepository.GetByIdAsync(idOrderPayment);
            if (payment is null)
            {
                return new PaymentMatchResult
                {
                    IsSuccess = false,
                    Error = PaymentOperationError.NotFound,
                    ErrorMessage = "Pago no encontrado."
                };
            }

            if (payment.IdStatus != PaymentUnmatchedStatusId)
            {
                return new PaymentMatchResult
                {
                    IsSuccess = false,
                    Error = PaymentOperationError.Validation,
                    ErrorMessage = "Solo se pueden asociar pagos que estén sin orden (Unmatched)."
                };
            }

            var order = await _paymentRepository.GetOrderByIdAsync(idOrder);
            if (order is null)
            {
                return new PaymentMatchResult
                {
                    IsSuccess = false,
                    Error = PaymentOperationError.NotFound,
                    ErrorMessage = "Orden no encontrada."
                };
            }

            if (order.IdStatus != OrderPendingStatusId &&
                order.IdStatus != OrderExpiredStatusId &&
                order.IdStatus != OrderUnderReviewStatusId)
            {
                return new PaymentMatchResult
                {
                    IsSuccess = false,
                    Error = PaymentOperationError.Validation,
                    ErrorMessage = "Solo se puede asociar el pago a una orden en espera (pendiente, expirada o en revisión)."
                };
            }

            payment.IdOrder = order.IdOrder;
            payment.IdStatus = PaymentApprovedStatusId;
            payment.RejectionReason = null;
            payment.ProcessedAt = DateTime.Now;

            order.IdStatus = OrderPaidStatusId;

            await _paymentRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Pago {IdOrderPayment} asociado a la orden {IdOrder}",
                payment.IdOrderPayment, order.IdOrder);

            return new PaymentMatchResult
            {
                IsSuccess = true,
                Message = "Pago asociado correctamente. El pago fue aprobado y la orden quedó pagada.",
                IdOrderPayment = payment.IdOrderPayment,
                IdOrder = order.IdOrder,
                OrderCode = order.OrderCode,
                OrderStatusId = order.IdStatus,
                PaymentStatusId = payment.IdStatus
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al asociar el pago {IdOrderPayment} a la orden {IdOrder}",
                idOrderPayment, idOrder);
            return new PaymentMatchResult
            {
                IsSuccess = false,
                Error = PaymentOperationError.Internal,
                ErrorMessage = "Error interno del servidor"
            };
        }
    }
}
