
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
    }
}
