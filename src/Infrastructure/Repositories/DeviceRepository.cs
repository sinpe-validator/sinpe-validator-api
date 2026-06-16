using System;
using System.Threading.Tasks;
using Application.Contracts;
using Infrastructure.Data;
using sinpe_validator_api.Domain.Entities;

namespace Infrastructure.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly SinpePaymentsDbContext _context;

        public DeviceRepository(SinpePaymentsDbContext context)
        {
            _context = context;
        }

        // Implementación del método que requiere tu interfaz IDeviceRepository
        public async Task AddHeartbeatAsync(DeviceHeartbeat heartbeat)
        {
            await _context.DeviceHeartbeats.AddAsync(heartbeat);
            await _context.SaveChangesAsync();
        }
    }
}
