using Application.Common.Results;
using Application.Contracts;

namespace Web.Endpoints.Payments;

public static class MatchPaymentToOrderHandler
{
    public static async Task<IResult> Handle(
        int idOrderPayment,
        MatchPaymentRequest request,
        IPaymentService paymentService)
    {
        var idOrder = request?.IdOrder ?? 0;
        var result = await paymentService.MatchPaymentToOrder(idOrderPayment, idOrder);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                PaymentOperationError.NotFound => Results.NotFound(new { error = result.ErrorMessage }),
                PaymentOperationError.Internal => Results.Problem(result.ErrorMessage),
                _ => Results.BadRequest(new { error = result.ErrorMessage })
            };
        }

        return Results.Ok(new
        {
            message = result.Message,
            idOrderPayment = result.IdOrderPayment,
            idOrder = result.IdOrder,
            orderCode = result.OrderCode,
            orderStatusId = result.OrderStatusId,
            paymentStatusId = result.PaymentStatusId
        });
    }
}

public class MatchPaymentRequest
{
    public int IdOrder { get; set; }
}
