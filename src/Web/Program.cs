using Application.Contracts;
using Application.Services;
using Application.Services.SmsValidation;
using Application.Services.SmsValidation.Validators;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddCors();
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
app.UseCors(static builder => 
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());

app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });

app.MapApiEndpoints();

app.Run();
