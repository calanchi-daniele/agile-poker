using AgilePoker.Api.Hubs;
using AgilePoker.Api.Services;
using AgilePoker.Api.Services.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IRoomManager, RoomManager>();
builder.Services.AddSingleton<ISimulationService, SimulationService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowViteFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://agile-poker-sable.vercel.app"
                ) 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowViteFrontend");
app.MapHub<PokerHub>("/AgilePoker");

app.Run();
