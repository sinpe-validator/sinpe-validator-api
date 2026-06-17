using Application.Contracts;
using Application.DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using sinpe_validator_api.Domain.Entities;

namespace sinpe_validator_api.Web.Endpoints.Sms;

public static class SendSmsHandler
{
    private const int PaymentStatusApproved = 1;
    private const int PaymentStatusRejected = 2;
    private const int PaymentStatusUnderReview = 3;
    private const int PaymentStatusUnmatched = 4;
    private const int OrderStatusPaid = 2;

    public static async Task<IResult> Handle(
        ReceivedSmsDto dto,
        SinpePaymentsDbContext dbContext,
        ISmsParsingService smsParser,
        ISmsValidationService validationService,
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

            var receivedAt = DateTime.Now;

            var receivedSms = new ReceivedSms
            {
                SenderName = parseResult.SenderName,
                Amount = parseResult.Amount,
                SinpeReference = parseResult.SinpeReference,
                Description = parseResult.Description,
                ReceivedAt = receivedAt
            };

            var validationResult = await validationService.ValidateSmsPaymentAsync(
                parseResult,
                receivedSms);

            if (!validationResult.IsValid && validationResult.ValidatorName == "DuplicateReferenceValidator")
            {
                logger.LogWarning(
                    "Referencia SINPE duplicada: {Reference}",
                    parseResult.SinpeReference);

                return Results.Conflict(new
                {
                    success = false,
                    status = "Rejected",
                    reason = validationResult.RejectionReason,
                    reference = parseResult.SinpeReference
                });
            }

            dbContext.ReceivedSms.Add(receivedSms);
            await dbContext.SaveChangesAsync();

            Order? order = null;
            var rejectionReason = validationResult.RejectionReason;

            if (!string.IsNullOrWhiteSpace(parseResult.Description))
            {
                var orderCode = parseResult.Description.Trim();
                order = await dbContext.Orders
                    .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

                if (order is not null && validationResult.OrderStatus.HasValue)
                {
                    order.IdStatus = validationResult.OrderStatus.Value;
                }
            }

            var paymentStatus = order is null
                ? PaymentStatusUnmatched
                : validationResult.PaymentStatus ?? PaymentStatusUnderReview;

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

            if (!validationResult.IsValid && validationResult.ValidatorName != "DuplicateReferenceValidator")
            {
                var inconsistencyType = validationResult.ValidatorName switch
                {
                    "AmountMatchValidator" => "AmountMismatch",
                    "OrderCodeValidator" => "InvalidOrderCode",
                    "PaymentDateValidator" => "DateMismatch",
                    _ => validationResult.ValidatorName ?? "Unknown"
                };

                dbContext.FraudAttempts.Add(new FraudAttempt
                {
                    IdSms = receivedSms.IdSms,
                    IdorderPayment = orderPayment.IdOrderPayment,
                    InconsistencyType = inconsistencyType,
                    Detail = validationResult.RejectionReason,
                    DetectedAt = DateTime.Now
                });
                await dbContext.SaveChangesAsync();
            }

            if (validationResult.IsValid && paymentStatus == PaymentStatusApproved)
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

            var statusName = paymentStatus switch
            {
                PaymentStatusRejected => "Rejected",
                PaymentStatusUnmatched => "Unmatched",
                _ => "UnderReview"
            };

            return Results.Ok(new
            {
                success = false,
                status = statusName,
                message = paymentStatus == PaymentStatusUnmatched
                    ? "El SMS fue recibido, pero no se encontró una orden asociada. Queda pendiente de asociación manual."
                    : "El SMS fue recibido, pero el pago no fue aprobado automáticamente.",
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
