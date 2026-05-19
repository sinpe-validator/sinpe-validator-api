using Application.Common.Results;
using Application.Contracts;
using Microsoft.Extensions.Logging;

namespace Application.Services.SmsValidation.Validators;

public class OrderCodeValidator : ISmsValidator
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderCodeValidator> _logger;

    public string ValidatorName => "OrderCodeValidator";

    public OrderCodeValidator(
        IOrderRepository orderRepository,
        ILogger<OrderCodeValidator> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<SmsValidationResult> ValidateAsync(
        SmsValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.ParseResult.Description))
        {
            _logger.LogWarning("SMS sin código de orden en la descripción");

            return new SmsValidationResult
            {
                IsValid = false,
                RejectionReason = "El SMS no contiene código de orden en la descripción.",
                ValidatorName = ValidatorName
            };
        }

        var orderCode = context.ParseResult.Description.Trim();

        var order = await _orderRepository.GetByCodeAndPendingStatusAsync(orderCode);

        if (order is null)
        {
            _logger.LogWarning(
                "Orden pendiente no encontrada para el código: {OrderCode}",
                orderCode);

            return new SmsValidationResult
            {
                IsValid = false,
                RejectionReason =
                    $"No existe una orden pendiente con el código '{orderCode}'.",
                ValidatorName = ValidatorName
            };
        }

        context.Order = order;

        return new SmsValidationResult
        {
            IsValid = true,
            ValidatorName = ValidatorName
        };
    }
}
