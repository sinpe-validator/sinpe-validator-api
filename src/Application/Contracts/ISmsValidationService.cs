using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Results;
using sinpe_validator_api.Domain.Entities;

namespace Application.Contracts;

public interface ISmsValidationService
{
    Task<SmsValidationResult> ValidateSmsPaymentAsync(
        SmsParsingResult parseResult,
        ReceivedSms receivedSms,
        CancellationToken cancellationToken = default);
}
