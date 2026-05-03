using AgilePoker.Api.Hubs;
using AgilePoker.Api.Services;
using AgilePoker.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IRoomManager, RoomManager>();
builder.Services.AddSingleton<ISimulationService, SimulationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapHub<PokerHub>("/AgilePoker");

app.Run();
