using Application.Contracts;
using Application.DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using sinpe_validator_api.Domain.Entities;

namespace sinpe_validator_api.Web.Endpoints.Sms;

public static class SendSmsHandler
{
    private const int OrderStatusPending = 1;
    private const int OrderStatusPaid = 2;
    private const int OrderStatusUnderReview = 4;

    private const int PaymentStatusApproved = 1;
    private const int PaymentStatusRejected = 2;
    private const int PaymentStatusUnderReview = 3;

    public static async Task<IResult> Handle(
        ReceivedSmsDto dto,
        SinpePaymentsDbContext dbContext,
        ISmsParsingService smsParser,
        ILogger<Program> logger)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.SmsContent))
            {
                return Results.BadRequest(new { error = "El contenido del SMS es obligatorio" });
            }

            var parseResult = smsParser.ParseSms(dto.SmsContent);

            if (!parseResult.IsValid)
            {
                logger.LogWarning("SMS inválido: {Error}", parseResult.ErrorMessage);

                return Results.BadRequest(new
                {
                    error = parseResult.ErrorMessage ?? "El SMS no es válido"
                });
            }

            var referenceExists = await dbContext.ReceivedSms
                .AnyAsync(s => s.SinpeReference == parseResult.SinpeReference);

            if (referenceExists)
            {
                logger.LogWarning(
                    "Referencia SINPE duplicada detectada: {Reference}",
                    parseResult.SinpeReference
                );

                return Results.Conflict(new
                {
                    success = false,
                    status = "Rejected",
                    reason = "La referencia SINPE ya fue registrada previamente.",
                    reference = parseResult.SinpeReference
                });
            }

         
            var receivedAt = DateTime.Now;

            var receivedSms = new ReceivedSms
            {
                SenderName = parseResult.SenderName,
                Amount = parseResult.Amount,
                SinpeReference = parseResult.SinpeReference,
                Description = parseResult.Description,
                ReceivedAt = receivedAt
            };

            dbContext.ReceivedSms.Add(receivedSms);
            await dbContext.SaveChangesAsync();

            Order? order = null;
            var paymentStatus = PaymentStatusUnderReview;
            string? rejectionReason = null;

            if (string.IsNullOrWhiteSpace(parseResult.Description))
            {
                rejectionReason = "El SMS no contiene código de orden en la descripción.";
            }
            else
            {
                var orderCode = parseResult.Description.Trim();

                order = await dbContext.Orders
                    .FirstOrDefaultAsync(o =>
                        o.OrderCode == orderCode &&
                        o.IdStatus == OrderStatusPending);

                if (order is null)
                {
                    rejectionReason = $"No existe una orden pendiente con el código '{orderCode}'.";
                }
                else if (parseResult.Amount != order.Amount)
                {
                    paymentStatus = PaymentStatusRejected;
                    order.IdStatus = OrderStatusUnderReview;

                    rejectionReason =
                        $"El monto recibido ({parseResult.Amount}) no coincide con el monto de la orden ({order.Amount}).";
                }
                else if (receivedAt < order.CreatedAt)
                {
                    paymentStatus = PaymentStatusUnderReview;
                    order.IdStatus = OrderStatusUnderReview;

                    rejectionReason =
                        $"Fecha inconsistente: el SMS fue recibido antes de la creación de la orden. Orden: {order.CreatedAt}, SMS: {receivedAt}.";
                }
                else if (receivedAt > order.ExpiresAt)
                {
                    paymentStatus = PaymentStatusUnderReview;
                    order.IdStatus = OrderStatusUnderReview;

                    rejectionReason =
                        $"Pago fuera de tiempo. La orden expiró en {order.ExpiresAt} y el SMS fue recibido en {receivedAt}.";
                }
                else
                {
                    paymentStatus = PaymentStatusApproved;
                    order.IdStatus = OrderStatusPaid;
                }
            }

            var orderPayment = new OrderPayment
            {
                IdOrder = order?.IdOrder,
                IdSms = receivedSms.IdSms,
                IdStatus = paymentStatus,
                RejectionReason = rejectionReason,
                ProcessedAt = DateTime.Now
            };

            dbContext.OrderPayments.Add(orderPayment);
            await dbContext.SaveChangesAsync();

            if (paymentStatus == PaymentStatusApproved)
            {
                logger.LogInformation(
                    "Pago aprobado. Orden: {OrderId}, Código: {OrderCode}, SMS: {SmsId}, Referencia: {Reference}",
                    order?.IdOrder,
                    order?.OrderCode,
                    receivedSms.IdSms,
                    receivedSms.SinpeReference
                );

                return Results.Ok(new
                {
                    success = true,
                    status = "Approved",
                    message = "Pago recibido, validado y asociado correctamente.",
                    smsId = receivedSms.IdSms,
                    orderId = order?.IdOrder,
                    orderCode = order?.OrderCode,
                    amount = receivedSms.Amount,
                    reference = receivedSms.SinpeReference,
                    senderName = receivedSms.SenderName
                });
            }

            logger.LogWarning(
                "Pago no aprobado automáticamente. SMS: {SmsId}, Referencia: {Reference}, Motivo: {Reason}",
                receivedSms.IdSms,
                receivedSms.SinpeReference,
                rejectionReason
            );

            return Results.Ok(new
            {
                success = false,
                status = paymentStatus == PaymentStatusRejected ? "Rejected" : "UnderReview",
                message = "El SMS fue recibido, pero el pago no fue aprobado automáticamente.",
                reason = rejectionReason,
                smsId = receivedSms.IdSms,
                orderId = order?.IdOrder,
                orderCode = order?.OrderCode,
                amount = receivedSms.Amount,
                reference = receivedSms.SinpeReference,
                senderName = receivedSms.SenderName
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando SMS");

            return Results.Problem(
                detail: "Error interno del servidor",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
