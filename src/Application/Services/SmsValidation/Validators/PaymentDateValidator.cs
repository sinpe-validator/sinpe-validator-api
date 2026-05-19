using Application.Common.Results;
using Application.Contracts;
using Microsoft.Extensions.Logging;

namespace Application.Services.SmsValidation.Validators;

public class PaymentDateValidator : ISmsValidator
{
    private const int OrderStatusUnderReview = 4;
    private const int PaymentStatusUnderReview = 3;

    private readonly ILogger<PaymentDateValidator> _logger;

    public string ValidatorName => "PaymentDateValidator";

    public PaymentDateValidator(ILogger<PaymentDateValidator> logger)
    {
        _logger = logger;
    }

    public Task<SmsValidationResult> ValidateAsync(
        SmsValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Order is null)
        {
            return Task.FromResult(new SmsValidationResult { IsValid = true, ValidatorName = ValidatorName });
        }

        var receivedAt = context.ReceivedSms.ReceivedAt;

        if (receivedAt < context.Order.CreatedAt)
        {
            _logger.LogWarning(
                "Fecha inconsistente - SMS recibido antes de la orden - Orden: {OrderDate}, SMS: {SmsDate}",
                context.Order.CreatedAt,
                receivedAt);

            context.Order.IdStatus = OrderStatusUnderReview;

            return Task.FromResult(new SmsValidationResult
            {
                IsValid = false,
                RejectionReason =
                    $"Fecha inconsistente: el SMS fue recibido antes de la creación de la orden. Orden: {context.Order.CreatedAt}, SMS: {receivedAt}.",
                PaymentStatus = PaymentStatusUnderReview,
                OrderStatus = OrderStatusUnderReview,
                ValidatorName = ValidatorName
            });
        }

        if (receivedAt > context.Order.ExpiresAt)
        {
            _logger.LogWarning(
                "Pago fuera de tiempo - Expiración: {ExpiryDate}, SMS recibido: {SmsDate}",
                context.Order.ExpiresAt,
                receivedAt);

            context.Order.IdStatus = OrderStatusUnderReview;

            return Task.FromResult(new SmsValidationResult
            {
                IsValid = false,
                RejectionReason =
                    $"Pago fuera de tiempo. La orden expiró en {context.Order.ExpiresAt} y el SMS fue recibido en {receivedAt}.",
                PaymentStatus = PaymentStatusUnderReview,
                OrderStatus = OrderStatusUnderReview,
                ValidatorName = ValidatorName
            });
        }

        return Task.FromResult(new SmsValidationResult
        {
            IsValid = true,
            ValidatorName = ValidatorName
        });
    }
}
