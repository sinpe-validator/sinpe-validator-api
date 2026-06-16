using System;
using System.Collections.Generic;
using System.Text;

namespace sinpe_validator_api.Domain.Entities;

public class FraudAttempt
{

    public int IdAttempt { get; set; }
    public int? IdSms { get; set; }
    public int? IdorderPayment { get; set; }
    public required string InconsistencyType { get; set; }

    public string? Detail { get; set; }

    public DateTime DetectedAt { get; set; }

}
