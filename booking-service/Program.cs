using booking_service.Data;
using booking_service.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<BookingDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHttpClient<EventServiceClient>(c =>
{
    var baseUrl = builder.Configuration["EventService:BaseUrl"] ?? "http://localhost:8080";
    c.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    c.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapGet("/health", async (BookingDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct)
        ? Results.Ok(new { status = "Healthy" })
        : Results.Json(new { status = "Unhealthy" }, statusCode: StatusCodes.Status503ServiceUnavailable));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
