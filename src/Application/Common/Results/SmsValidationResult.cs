using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Results;

public class SmsValidationResult
{
    public bool IsValid { get; set; }
    public string? RejectionReason { get; set; }
    public int? PaymentStatus { get; set; }
    public int? OrderStatus { get; set; }
    public string ValidatorName { get; set; } = string.Empty;
}
