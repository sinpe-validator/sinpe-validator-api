using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using sinpe_validator_api.Domain.Entities; // Asegúrate de tener la entidad mapeada

namespace Web.Endpoints.Heartbeat;

public static class ReceiveHeartbeatHandler
{
    public class HeartbeatRequestDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }

    public static async Task<IResult> Handle(
    HeartbeatRequestDto request,
    SinpePaymentsDbContext dbContext,
    ILogger<Program> logger)
    {
        try
        {
            // 1. Procesar el tiempo en UTC
            DateTime receivedAt = DateTimeOffset.TryParse(request.Timestamp, out var dto)
                                  ? dto.UtcDateTime
                                  : DateTime.UtcNow;

            // 2. Creamos el registro SIN asignar IdDevice. 
            // SQL Server generará el ID automáticamente al hacer el INSERT.
            var nuevoRegistro = new DeviceHeartbeat
            {
                Name = "Main Phone",
                LastConnection = receivedAt,
                IsActive = true
            };

            // 3. Guardar
            dbContext.DeviceHeartbeats.Add(nuevoRegistro);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Heartbeat guardado exitosamente por la BD (Autoincremental).");

            return Results.Ok(new { success = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error insertando nuevo registro de heartbeat");
            return Results.Problem();
        }
    }
}
