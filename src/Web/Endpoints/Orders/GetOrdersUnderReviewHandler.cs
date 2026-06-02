using Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Web.Endpoints.Orders;

public static class GetOrdersUnderReviewHandler
{
    private const int OrderUnderReviewStatusId = 4;
    private const int PaymentUnderReviewStatusId = 3;

    public static async Task<Ok<List<OrderUnderReviewResponse>>> Handle(
        SinpePaymentsDbContext context)
    {
        var orders = await context.OrderPayments
            .Include(payment => payment.Order)
            .Include(payment => payment.Sms)
            .Include(payment => payment.Status)
            .Where(payment =>
                payment.IdStatus == PaymentUnderReviewStatusId &&
                payment.Order != null &&
                payment.Order.IdStatus == OrderUnderReviewStatusId)
            .OrderByDescending(payment => payment.ProcessedAt)
           .Select(payment => new OrderUnderReviewResponse
           {
               IdOrderPayment = payment.IdOrderPayment,
               IdOrder = payment.Order!.IdOrder,
               OrderCode = payment.Order.OrderCode,
               OrderAmount = payment.Order.Amount,
               OrderDescription = payment.Order.Description,
               CreatedAt = payment.Order.CreatedAt,
               OrderExpiresAt = payment.Order.ExpiresAt,
               OrderStatusId = payment.Order.IdStatus,

               IdSms = payment.IdSms,
               SenderName = payment.Sms != null ? payment.Sms.SenderName : null,
               SmsAmount = payment.Sms != null ? payment.Sms.Amount : null,
               SinpeReference = payment.Sms != null ? payment.Sms.SinpeReference : null,
               SmsDescription = payment.Sms != null ? payment.Sms.Description : null,
               SmsRegisteredAt = payment.Sms != null ? payment.Sms.RegisteredAt : null,

               PaymentStatusId = payment.IdStatus,
               PaymentStatusName = payment.Status != null ? payment.Status.Name : null,
               RejectionReason = payment.RejectionReason,
               ProcessedAt = payment.ProcessedAt
           })
            .ToListAsync();

        return TypedResults.Ok(orders);
    }
}

public class OrderUnderReviewResponse
{
    public int IdOrderPayment { get; set; }

    public int IdOrder { get; set; }
    public string? OrderCode { get; set; }
    public decimal OrderAmount { get; set; }
    public string? OrderDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime OrderExpiresAt { get; set; }
    public int OrderStatusId { get; set; }

    public int IdSms { get; set; }
    public string? SenderName { get; set; }
    public decimal? SmsAmount { get; set; }
    public string? SinpeReference { get; set; }
    public string? SmsDescription { get; set; }
    public DateTime? SmsRegisteredAt { get; set; }

    public int PaymentStatusId { get; set; }
    public string? PaymentStatusName { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime ProcessedAt { get; set; }
}
