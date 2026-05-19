using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Results;

namespace Application.Contracts;

public interface ISmsValidator
{
    Task<SmsValidationResult> ValidateAsync(
        SmsValidationContext context,
        CancellationToken cancellationToken = default);
    string ValidatorName { get; }
}
