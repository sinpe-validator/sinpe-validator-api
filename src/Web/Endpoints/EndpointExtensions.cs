
using Application.DTOs;
using sinpe_validator_api.Web.Endpoints.Orders;
using sinpe_validator_api.Web.Endpoints.Sms;
namespace Web.Endpoints;

public static class EndpointExtensions
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var smsGroup = app.MapGroup("/api/sms")
            .WithName("SMS");

        smsGroup.MapPost("/", SendSmsHandler.Handle)
            .WithName("EnviarSMS")
            .WithDescription("Recibe un SMS crudo desde la aplicación Android y lo procesa")
            .Produces(200)
            .Produces(400)
            .Produces(500);

        // el de ordenes xd: 

        var ordersGroup = app.MapGroup("/api/orders")
    .WithName("Orders");

        ordersGroup.MapPost("/", CreateOrderHandler.Handle)
            .WithName("CreateOrder")
            .WithDescription("Genera una orden de pago pendiente con código de confirmación")
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        ordersGroup.MapGet("/", GetOrdersHandler.Handle)
    .WithName("GetOrders")
    .WithDescription("Lista las órdenes de pago generadas")
    .Produces<List<OrderDto>>(StatusCodes.Status200OK);

    }
}
