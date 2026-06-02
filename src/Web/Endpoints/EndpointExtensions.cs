using Application.DTOs;
using sinpe_validator_api.Web.Endpoints.Orders;
using sinpe_validator_api.Web.Endpoints.Sms;
using Web.Endpoints.Orders;

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

        // Endpoints de órdenes

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

        ordersGroup.MapGet("/{id:int}", GetOrderByIdHandler.Handle)
            .WithName("GetOrderById")
            .WithDescription("Retorna una orden por su ID")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        ordersGroup.MapGet("/under-review", GetOrdersUnderReviewHandler.Handle)
            .WithName("GetOrdersUnderReview")
            .WithDescription("Lista las órdenes que están en revisión manual junto con el pago y SMS asociado")
            .Produces<List<OrderUnderReviewResponse>>(StatusCodes.Status200OK);

        ordersGroup.MapPost("/{idOrder:int}/accept-payment", AcceptOrderPaymentHandler.Handle)
            .WithName("AcceptOrderPayment")
            .WithDescription("Acepta manualmente una orden en revisión, aprueba el pago asociado y marca la orden como pagada")
            .Produces<ManualOrderReviewResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        ordersGroup.MapPost("/{idOrder:int}/reject-payment", RejectOrderPaymentHandler.Handle)
            .WithName("RejectOrderPayment")
            .WithDescription("Rechaza manualmente una orden en revisión, rechaza el pago asociado y guarda la razón del rechazo")
            .Produces<ManualOrderReviewResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        ordersGroup.MapPatch("/{id:int}/expire", ExpireOrderHandler.Handle)
            .WithName("ExpireOrder")
            .WithDescription("Marca una orden como expirada")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
