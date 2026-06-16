using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Application.Contracts;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Workers;

public class HeartbeatMonitorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HeartbeatMonitorWorker> _logger;
    // Inicializamos explícitamente para evitar errores de referencia nula
    private readonly ConcurrentDictionary<int, bool> _notified = new ConcurrentDictionary<int, bool>();

    public HeartbeatMonitorWorker(IServiceProvider serviceProvider, ILogger<HeartbeatMonitorWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SinpePaymentsDbContext>();
                var notifier = scope.ServiceProvider.GetRequiredService<INotificationSender>();

                var lastEntry = await db.DeviceHeartbeats
                    .OrderByDescending(d => d.LastConnection)
                    .FirstOrDefaultAsync(stoppingToken);

                // La lógica de "caído" se basa en si el último registro es antiguo
                bool estaCaido = lastEntry != null && lastEntry.LastConnection < DateTime.UtcNow.AddMinutes(-2);

                if (estaCaido)
                {
                    if (!_notified.ContainsKey(1))
                    {
                        _logger.LogCritical("DISPOSITIVO CAÍDO - Notificando al POS...");
                        await Task.Delay(5000);

                        if (notifier != null)
                        {
                            await notifier.SendToGroupAsync("POS", "ReceiveNotification", new
                            {
                                DeviceId = "MainPhone",
                                Message = "DeviceDown"
                            }, stoppingToken);
                        }

                        _notified[1] = true;
                    }
                }
                else if (lastEntry != null)
                {
                    // Limpieza segura del estado
                    if (_notified.ContainsKey(1))
                    {
                        _logger.LogInformation("Dispositivo recuperado. Limpiando estado de alerta.");
                        _notified.TryRemove(1, out _);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en monitor de heartbeat");
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}
