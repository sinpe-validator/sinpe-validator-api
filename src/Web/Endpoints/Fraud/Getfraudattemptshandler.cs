using Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Web.Endpoints.Fraud;

public static class GetFraudAttemptsHandler
{
    public static async Task<Ok<List<FraudAttemptResponse>>> Handle(
        SinpePaymentsDbContext context)
    {
        var attempts = await context.FraudAttempts
            .OrderByDescending(f => f.DetectedAt)
            .Select(f => new FraudAttemptResponse
            {
                IdAttempt = f.IdAttempt,
                IdSms = f.IdSms,
                IdOrderPayment = f.IdorderPayment,
                InconsistencyType = f.InconsistencyType,
                Detail = f.Detail,
                DetectedAt = f.DetectedAt
            })
            .ToListAsync();

        return TypedResults.Ok(attempts);
    }
}

public class FraudAttemptResponse
{
    public int IdAttempt { get; set; }
    public int? IdSms { get; set; }
    public int? IdOrderPayment { get; set; }
    public required string InconsistencyType { get; set; }
    public string? Detail { get; set; }
    public DateTime DetectedAt { get; set; }
}
