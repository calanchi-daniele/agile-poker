using AgilePoker.Api.Hubs;
using AgilePoker.Api.Services;
using AgilePoker.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IRoomManager, RoomManager>();
builder.Services.AddSingleton<ISimulationService, SimulationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowViteFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // CRITICAL for SignalR WebSockets
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowViteFrontend");
app.MapHub<PokerHub>("/AgilePoker");

app.Run();
