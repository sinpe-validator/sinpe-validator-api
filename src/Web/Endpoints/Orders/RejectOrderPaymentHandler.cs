using Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Web.Endpoints.Orders;

public static class RejectOrderPaymentHandler
{
    private const int OrderPendingStatusId = 1;
    private const int OrderExpiredStatusId = 3;
    private const int OrderUnderReviewStatusId = 4;

    private const int PaymentRejectedStatusId = 2;
    private const int PaymentUnderReviewStatusId = 3;

    public static async Task<Results<
        Ok<ManualOrderReviewResponse>,
        NotFound<string>,
        BadRequest<string>>> Handle(
        int idOrder,
        RejectOrderPaymentRequest request,
        SinpePaymentsDbContext context)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Reason))
        {
            return TypedResults.BadRequest("Debe indicar una razón de rechazo.");
        }

        var order = await context.Orders
            .FirstOrDefaultAsync(order => order.IdOrder == idOrder);

        if (order is null)
        {
            return TypedResults.NotFound("Orden no encontrada.");
        }

        if (order.IdStatus != OrderUnderReviewStatusId)
        {
            return TypedResults.BadRequest("Solo se pueden rechazar órdenes que estén en revisión manual.");
        }

        var payment = await context.OrderPayments
            .Where(payment =>
                payment.IdOrder == idOrder &&
                payment.IdStatus == PaymentUnderReviewStatusId)
            .OrderByDescending(payment => payment.ProcessedAt)
            .FirstOrDefaultAsync();

        if (payment is null)
        {
            return TypedResults.BadRequest("La orden no tiene un pago en revisión asociado.");
        }

        payment.IdStatus = PaymentRejectedStatusId;
        payment.RejectionReason = request.Reason.Trim();
        payment.ProcessedAt = DateTime.Now;

        if (order.ExpiresAt <= DateTime.Now)
        {
            order.IdStatus = OrderExpiredStatusId;
        }
        else
        {
            order.IdStatus = OrderPendingStatusId;
        }

        await context.SaveChangesAsync();

        var response = new ManualOrderReviewResponse
        {
            Message = "Orden rechazada manualmente. El pago fue rechazado y la orden no quedó pagada.",
            IdOrder = order.IdOrder,
            IdOrderPayment = payment.IdOrderPayment,
            OrderStatusId = order.IdStatus,
            PaymentStatusId = payment.IdStatus
        };

        return TypedResults.Ok(response);
    }
}

public class RejectOrderPaymentRequest
{
    public string Reason { get; set; } = string.Empty;
}
