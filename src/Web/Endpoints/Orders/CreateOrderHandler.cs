namespace sinpe_validator_api.Web.Endpoints.Orders;

using Application.DTOs;
using global::sinpe_validator_api.Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;


public static class CreateOrderHandler
{
    private const int PendingStatusId = 1;
    private const int ExpirationMinutes = 5;

    public static async Task<Results<Created<OrderDto>, BadRequest<object>, ProblemHttpResult>> Handle(
        CreateOrderDto request,
        SinpePaymentsDbContext dbContext,
        ILogger<Program> logger)
    {
        try
        {
            if (request.Amount <= 0)
            {
                return TypedResults.BadRequest<object>(new { message = "El monto de la orden debe ser mayor a cero." });
            }

            var now = DateTime.Now;

            var orderCode = await GenerateUniqueOrderCodeAsync(dbContext);

            var order = new Order
            {
                OrderCode = orderCode,
                Amount = request.Amount,
                IdStatus = PendingStatusId,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(ExpirationMinutes),
                Description = request.Description
            };

            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            var response = new OrderDto
            {
                IdOrder = order.IdOrder,
                OrderCode = order.OrderCode ?? string.Empty,
                Amount = order.Amount,
                Status = "Pending",
                CreatedAt = order.CreatedAt,
                ExpiresAt = order.ExpiresAt,
                Description = order.Description
            };

            logger.LogInformation(
                "Orden creada correctamente. IdOrder: {IdOrder}, OrderCode: {OrderCode}, Amount: {Amount}",
                order.IdOrder,
                order.OrderCode,
                order.Amount
            );

            return TypedResults.Created($"/api/orders/{order.IdOrder}", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear la orden de pago.");

            return TypedResults.Problem(
                title: "Error al crear la orden de pago",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    private static async Task<string> GenerateUniqueOrderCodeAsync(SinpePaymentsDbContext dbContext)
    {
        const int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            var code = Random.Shared.Next(0, 1_000_000).ToString("D6");

            var exists = await dbContext.Orders.AnyAsync(o =>
                o.OrderCode == code &&
                o.IdStatus == PendingStatusId);

            if (!exists)
            {
                return code;
            }
        }

        throw new InvalidOperationException("No se pudo generar un código único para la orden.");
    }
}
