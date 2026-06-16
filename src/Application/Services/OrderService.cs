using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Results;
using Application.Contracts;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class OrderService : IOrderService
{
    private const int OrderStatusExpired = 3;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }
    public async Task<OrderServiceResult> ExpireOrder(int orderId)
    {
        try
        {
            var success = await _orderRepository.MarkAsExpiredAsync(orderId, OrderStatusExpired);
            if (!success)
            {
                _logger.LogWarning("Orden no encontrada: {OrdenId}", orderId);
                return new OrderServiceResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Orden con ID {orderId} no encontrada."
                };
            }

            _logger.LogInformation("Orden marcada como expirada: {OrdenId}", orderId);
            return new OrderServiceResult
            {
                IsSuccess = true,
                Message = "La orden ha sido marcada como expirada",
                OrderId = orderId
            };
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error al marcar orden como expirada: {OrderId}", orderId);
            return new OrderServiceResult
            {
                IsSuccess = false,
                ErrorMessage = "Error interno del servidor"
            };
        }
    }
}
