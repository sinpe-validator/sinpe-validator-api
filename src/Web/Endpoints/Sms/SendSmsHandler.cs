using Application.Contracts;
using Application.DTOs;
using Application.Services;
using Infrastructure.Data;
using sinpe_validator_api.Domain.Entities;

namespace sinpe_validator_api.Web.Endpoints.Sms;

public static class SendSmsHandler
{
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
                logger.LogWarning($"SMS inválido: {parseResult.ErrorMessage}");
                return Results.BadRequest(new { error = parseResult.ErrorMessage ?? "El SMS no es válido" });
            }

            var receivedSms = new ReceivedSms
            {
                SenderName = parseResult.SenderName,
                Amount = parseResult.Amount,
                SinpeReference = parseResult.SinpeReference,
                Description = parseResult.Description,
                ReceivedAt = DateTime.UtcNow
            };

            dbContext.ReceivedSms.Add(receivedSms);
            await dbContext.SaveChangesAsync();

            logger.LogInformation($"SMS procesado exitosamente - Referencia: {parseResult.SinpeReference}, Monto: {parseResult.Amount}");

            return Results.Ok(new
            {
                success = true,
                message = "SMS recibido y procesado exitosamente",
                smsId = receivedSms.IdSms,
                amount = parseResult.Amount,
                reference = parseResult.SinpeReference,
                senderName = parseResult.SenderName
            });
        }
        catch (Exception ex) 
        {
            logger.LogError($"Error procesando SMS: {ex.Message}");
            return Results.Problem(
                detail: "Error interno del servidor",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
