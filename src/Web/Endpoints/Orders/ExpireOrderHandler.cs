using Application.Contracts;
using Infrastructure.Data;

namespace sinpe_validator_api.Web.Endpoints.Orders;

public static class ExpireOrderHandler
{
    public static async Task<IResult> Handle(
        int id,
        IOrderService orderService)
    {
        var result = await orderService.ExpireOrder(id);
        if (!result.IsSuccess)
        {
            return Results.NotFound(new { error = result.ErrorMessage });
        }
        return Results.Ok(new 
        { 
            success = result.IsSuccess, 
            message = result.Message,
            OrderId = result.OrderId
        });
    }
}
