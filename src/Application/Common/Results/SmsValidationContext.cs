using System;
using System.Collections.Generic;
using System.Text;
using sinpe_validator_api.Domain.Entities;

namespace Application.Common.Results;

public class SmsValidationContext
{
    public required SmsParsingResult ParseResult { get; set; }
    public required ReceivedSms ReceivedSms { get; set; }
    public Order? Order { get; set; }
}
