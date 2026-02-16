using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using TcpServerApp1.Alert_store;
using TcpServerApp1.RealTime;
using TcpServerApp1.Tcp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<LiveDataBroadcaster>();

builder.Services.AddSingleton<AlertQueue>();

builder.Services.AddHostedService<TcpServerService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

app.UseAuthorization();
app.UseWebSockets();


app.Map("/dashboard", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var broadcaster = context.RequestServices.GetRequiredService<LiveDataBroadcaster>();
    var socket = await context.WebSockets.AcceptWebSocketAsync();
    var socketId = Guid.NewGuid().ToString();

    broadcaster.AddSocket(socketId, socket);

    var buffer = new byte[4096];

    try
    {
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
                break;
        }
    }
    finally
    {
        broadcaster.RemoveSocket(socketId);

        if (socket.State == WebSocketState.Open ||
            socket.State == WebSocketState.CloseReceived)
        {
            try
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closed",
                    CancellationToken.None);
            } catch { }
        }
    }
});

app.MapControllers();
app.Run();