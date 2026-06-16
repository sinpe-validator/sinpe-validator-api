using System.Threading;
using System.Threading.Tasks;
using Application.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Web.Services;

public class SignalRNotificationSender : INotificationSender
{
    private readonly IHubContext<Web.Hubs.NotificationHub> _hubContext;

    public SignalRNotificationSender(IHubContext<Web.Hubs.NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendToGroupAsync(string groupName, string method, object payload, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.Group(groupName).SendAsync(method, payload, cancellationToken);
    }
}
