using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface INotificationSender
    {
        Task SendToGroupAsync(string groupName, string method, object payload, CancellationToken cancellationToken = default);
    }
}
