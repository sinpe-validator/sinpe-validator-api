using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Results;
using Application.Contracts;
using Microsoft.Extensions.Logging;
using sinpe_validator_api.Domain.Entities;

namespace Application.Services.SmsValidation;

public class SmsValidationService : ISmsValidationService
{
    private readonly IEnumerable<ISmsValidator> _validators;
    private readonly ILogger<SmsValidationService> _logger;

    public SmsValidationService(
        IEnumerable<ISmsValidator> validators,
        ILogger<SmsValidationService> logger)
    {
        _validators = validators;
        _logger = logger;
    }
    public async Task<SmsValidationResult> ValidateSmsPaymentAsync(SmsParsingResult parseResult, ReceivedSms receivedSms, CancellationToken cancellationToken = default)
    {
        var context = new SmsValidationContext
        {
            ParseResult = parseResult,
            ReceivedSms = receivedSms
        };

        foreach (var validator in _validators) 
        {
            _logger.LogInformation("Ejecutando validador: {ValidatorName}", validator.ValidatorName);
            
            var result = await validator.ValidateAsync(context, cancellationToken);

            if (!result.IsValid)
            {
                _logger.LogWarning(
                    "Validación fallida en {ValidatorName}: {Reason}",
                    validator.ValidatorName,
                    result.RejectionReason);
                return result;
            }
        }

        _logger.LogInformation("Todas las validaciones pasaron correctamente");

        return new SmsValidationResult
        {
            IsValid = true,
            PaymentStatus = 1,
            OrderStatus = 2
        };
    }
}
