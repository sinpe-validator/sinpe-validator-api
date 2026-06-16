using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Aquí podemos decidir si el cliente se une al grupo POS o a otro grupo
            // Por ejemplo, el cliente POS al conectarse puede indicar en la querystring o headers
            // que es un POS; para simplicidad, si la conexión tiene ?role=pos lo añadimos al grupo POS
            var httpContext = Context.GetHttpContext();
            if (httpContext != null && httpContext.Request.Query.TryGetValue("role", out var role))
            {
                if (string.Equals(role, "pos", StringComparison.OrdinalIgnoreCase))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "POS");
                }
            }

            await base.OnConnectedAsync();
        }
    }
}
