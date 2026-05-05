using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Results;

namespace Application.Contracts;

public interface ISmsParsingService
{
    SmsParsingResult ParseSms(string smsContent);
}
