using Application.Contracts;
using Application.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Web.Endpoints.Payments;

public static class GetUnmatchedPaymentsHandler
{
    public static async Task<Ok<List<UnmatchedPaymentDto>>> Handle(
        IPaymentService paymentService)
    {
        var payments = await paymentService.GetUnmatchedPayments();
        return TypedResults.Ok(payments);
    }
}
