namespace Application.Contracts
{
    using sinpe_validator_api.Domain.Entities;

    public interface IDeviceRepository
    {
        Task AddHeartbeatAsync(DeviceHeartbeat heartbeat);
    }
}
