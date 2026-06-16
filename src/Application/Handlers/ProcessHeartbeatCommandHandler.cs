using System;
using System.Threading.Tasks;
using Application.Contracts;
using Application.Commands;
using sinpe_validator_api.Domain.Entities;

namespace Application.Handlers
{
    public class ProcessHeartbeatCommandHandler
    {
        private readonly IDeviceRepository _repository;

        public ProcessHeartbeatCommandHandler(IDeviceRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(ProcessHeartbeatCommand command)
        {
            // Convertimos el ID a int (ya que tu BD usa int para IdDevice)
            int deviceId = int.Parse(command.DeviceId);
            DateTime timestamp = DateTime.Parse(command.Timestamp);

            // Creamos un nuevo registro de heartbeat directamente
            var heartbeat = new DeviceHeartbeat
            {
                IdDevice = deviceId,
                Name = "Main Phone", // O el nombre que recibas
                LastConnection = timestamp,
                IsActive = true
            };

            await _repository.AddHeartbeatAsync(heartbeat);
        }
    }
}