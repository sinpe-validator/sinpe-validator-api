using Scalar.AspNetCore;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddControllers();

//BD
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

// Map controller routes
app.MapControllers();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });

app.Run();
