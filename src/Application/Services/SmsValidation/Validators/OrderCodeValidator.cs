using Application.Common.Results;
using Application.Contracts;
using Microsoft.Extensions.Logging;

namespace Application.Services.SmsValidation.Validators;

/// <summary>
/// Valida que exista una orden con el código proporcionado
/// </summary>
public class OrderCodeValidator : ISmsValidator
{
    private const int OrderStatusPending = 1;
    private const int OrderStatusExpired = 3;
    private const int OrderStatusUnderReview = 4;

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

        var order = await _orderRepository.GetByCodeAsync(orderCode);

        if (order is null)
        {
            _logger.LogWarning(
                "Orden no encontrada para el código: {OrderCode}",
                orderCode);

            return new SmsValidationResult
            {
                IsValid = false,
                RejectionReason =
                    $"No existe una orden con el código '{orderCode}'.",
                ValidatorName = ValidatorName
            };
        }

        if (order.IdStatus != OrderStatusPending && 
            order.IdStatus != OrderStatusExpired && 
            order.IdStatus != OrderStatusUnderReview)
        {
            _logger.LogWarning(
                "Orden con estado no permitido para validación - Código: {OrderCode}, Estado: {Status}",
                orderCode,
                order.IdStatus);

            return new SmsValidationResult
            {
                IsValid = false,
                RejectionReason =
                    $"La orden con código '{orderCode}' no puede recibir pagos en este momento.",
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
