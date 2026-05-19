using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Results;
using Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using sinpe_validator_api.Domain.Entities;

namespace Application.Services.SmsValidation.Validators;

public class DuplicateReferenceValidator : ISmsValidator
{
    private readonly ISmsRepository _smsRepository;
    private readonly ILogger<DuplicateReferenceValidator> _logger;

    public string ValidatorName => "DuplicateReferenceValidator";

    public DuplicateReferenceValidator(
        ISmsRepository smsRepository,
        ILogger<DuplicateReferenceValidator> logger)
    {
        _smsRepository = smsRepository;
        _logger = logger;
    }

    public async Task<SmsValidationResult> ValidateAsync(
        SmsValidationContext context,
        CancellationToken cancellationToken = default)
    {
        var referenceExists = await _smsRepository.ReferenceExistsAsync(context.ParseResult.SinpeReference);

        if (referenceExists)
        {
            _logger.LogWarning(
                "Referencia SINPE duplicada detectada: {Reference}",
                context.ParseResult.SinpeReference);

            return new SmsValidationResult
            {
                IsValid = false,
                RejectionReason =
                    $"La referencia {context.ParseResult.SinpeReference} ya fue registrada.",
                ValidatorName = ValidatorName
            };
        }

        return new SmsValidationResult
        {
            IsValid = true,
            ValidatorName = ValidatorName
        };
    }
}
