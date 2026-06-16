using Application.Contracts;
using Application.Services;
using Application.Services.SmsValidation;
using Application.Services.SmsValidation.Validators;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Configurar CORS con origenes explícitos y AllowCredentials para SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // FRONTEND origin (ajusta según tu entorno)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddScoped<ISmsParsingService, SmsParsingService>();
builder.Services.AddScoped<ISmsValidationService, SmsValidationService>();
builder.Services.AddScoped<ISmsValidator, DuplicateReferenceValidator>();
builder.Services.AddScoped<ISmsValidator, OrderCodeValidator>();
builder.Services.AddScoped<ISmsValidator, AmountMatchValidator>();
builder.Services.AddScoped<ISmsValidator, PaymentDateValidator>();
builder.Services.AddScoped<ISmsRepository, SmsRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddHostedService<HeartbeatMonitorWorker>();
builder.Services.AddSignalR();
// Registrar NotificationHub
builder.Services.AddSingleton<Web.Hubs.NotificationHub>();
// Registrar el sender concreto para SignalR
builder.Services.AddScoped<Application.Contracts.INotificationSender, Web.Services.SignalRNotificationSender>();

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SinpePaymentsDbContext>(options =>
    options.UseSqlServer(connectionString)
);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<SinpePaymentsDbContext>();
        
        if (dbContext.Database.CanConnect())
        {
            logger.LogInformation("CONEXIÓN A BASE DE DATOS EXITOSA");
        }
        else
        {
            logger.LogError("NO SE PUDO CONECTAR A LA BASE DE DATOS");
        }
    }
}
catch (Exception ex)
{
    logger.LogError($"ERROR AL CONECTAR: {ex.Message}");
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });

app.MapApiEndpoints();

// Mapear hub de notificaciones (debe ir después de UseCors)
app.MapHub<Web.Hubs.NotificationHub>("/hubs/notifications");

app.Run();
