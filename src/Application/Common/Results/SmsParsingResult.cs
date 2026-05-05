using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Results;

public class SmsParsingResult
{
    public bool IsValid { get; set; }
    public decimal Amount { get; set; }
    public string SinpeReference { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ErrorMessage { get; set; }
}
