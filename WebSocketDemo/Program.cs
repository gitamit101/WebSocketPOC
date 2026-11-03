using WebSocketDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<WebSocketDemo.Services.WebSocketManager>();

var app = builder.Build();

app.UseHttpsRedirection();

// Enable WebSocket support
app.UseWebSockets();

// WebSocket endpoint
app.Map("/ws", async (
    HttpContext context,
    WebSocketDemo.Services.WebSocketManager manager) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    // Accept connection
    var socket = await context.WebSockets.AcceptWebSocketAsync();

    // Register client
    var clientId = manager.AddClient(socket);

    // Keep connection alive
    await manager.HandleConnectionAsync(
        clientId,
        socket,
        context.RequestAborted);
});

app.MapControllers();

app.Run();