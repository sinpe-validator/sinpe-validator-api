using Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Web.Endpoints.Orders;

public static class AcceptOrderPaymentHandler
{
    private const int OrderPaidStatusId = 2;
    private const int OrderExpiredStatusId = 3;
    private const int OrderUnderReviewStatusId = 4;

    private const int PaymentApprovedStatusId = 1;
    private const int PaymentUnderReviewStatusId = 3;

    public static async Task<Results<
        Ok<ManualOrderReviewResponse>,
        NotFound<string>,
        BadRequest<string>>> Handle(
        int idOrder,
        SinpePaymentsDbContext context)
    {
        var order = await context.Orders
            .FirstOrDefaultAsync(order => order.IdOrder == idOrder);

        if (order is null)
        {
            return TypedResults.NotFound("Orden no encontrada.");
        }

        if (order.IdStatus != OrderExpiredStatusId && order.IdStatus != OrderUnderReviewStatusId)
        {
            return TypedResults.BadRequest("Solo se pueden aceptar órdenes que estén expiradas o en revisión manual.");
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

        order.IdStatus = OrderPaidStatusId;

        payment.IdStatus = PaymentApprovedStatusId;
        payment.RejectionReason = null;
        payment.ProcessedAt = DateTime.Now;

        await context.SaveChangesAsync();

        var response = new ManualOrderReviewResponse
        {
            Message = "Orden aceptada manualmente. El pago fue aprobado y la orden quedó pagada.",
            IdOrder = order.IdOrder,
            IdOrderPayment = payment.IdOrderPayment,
            OrderStatusId = order.IdStatus,
            PaymentStatusId = payment.IdStatus
        };

        return TypedResults.Ok(response);
    }
}

public class ManualOrderReviewResponse
{
    public string Message { get; set; } = string.Empty;
    public int IdOrder { get; set; }
    public int IdOrderPayment { get; set; }
    public int OrderStatusId { get; set; }
    public int PaymentStatusId { get; set; }
}
