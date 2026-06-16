using Application.Common.Results;
using Application.Contracts;
using Microsoft.Extensions.Logging;

namespace Application.Services.SmsValidation.Validators;
public class AmountMatchValidator : ISmsValidator
{
    private const int OrderStatusUnderReview = 4;
    private const int PaymentStatusRejected = 2;

    private readonly ILogger<AmountMatchValidator> _logger;

    public string ValidatorName => "AmountMatchValidator";

    public AmountMatchValidator(ILogger<AmountMatchValidator> logger)
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

        if (context.ParseResult.Amount != context.Order.Amount)
        {
            _logger.LogWarning(
                "Monto no coincide - Recibido: {ReceivedAmount}, Esperado: {ExpectedAmount}",
                context.ParseResult.Amount,
                context.Order.Amount);

            context.Order.IdStatus = OrderStatusUnderReview;

            return Task.FromResult(new SmsValidationResult
            {
                IsValid = false,
                RejectionReason =
                    $"El monto recibido ({context.ParseResult.Amount}) no coincide con el monto de la orden ({context.Order.Amount}).",
                PaymentStatus = PaymentStatusRejected,
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
