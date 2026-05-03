using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(ILogger<PaymentController> logger)
    {
        _logger = logger;
    }

    //Endpoint que recibe un SMS con el pago recibido, lo loguea y devuelve el DTO recibido
    [HttpPost]
    public IActionResult ReceivePayment([FromBody] ReceivedSmsDto dto)
    {
        if (dto is null)
        {
            return BadRequest("Request body is required.");
        }

        var json = System.Text.Json.JsonSerializer.Serialize(dto); 
        Console.WriteLine("Mensaje nuevo recibido: " + json);
        return Created(string.Empty, dto);
    }
}
