using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketDemo.Services
{
    public class WebSocketManager
    {
        private class ClientConnection
        {
            public WebSocket Socket { get; }

            public SemaphoreSlim SendLock { get; } = new(1, 1);

            public ClientConnection(WebSocket socket)
            {
                Socket = socket;
            }
        }

        private readonly ConcurrentDictionary<Guid, ClientConnection>
            _clients = new();

        // Add connected client
        public Guid AddClient(WebSocket socket)
        {
            var clientId = Guid.NewGuid();

            _clients.TryAdd(
                clientId,
                new ClientConnection(socket));

            Console.WriteLine($"Client connected: {clientId}");

            return clientId;
        }

        // Remove disconnected client
        public void RemoveClient(Guid clientId)
        {
            if (_clients.TryRemove(clientId, out var client))
            {
                client.SendLock.Dispose();

                Console.WriteLine($"Client disconnected: {clientId}");
            }
        }

        // Broadcast message to all connected clients
        public async Task BroadcastAsync(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);

            foreach (var client in _clients.Values)
            {
                await client.SendLock.WaitAsync();

                try
                {
                    if (client.Socket.State == WebSocketState.Open)
                    {
                        await client.Socket.SendAsync(
                            new ArraySegment<byte>(messageBytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    client.SendLock.Release();
                }
            }
        }

        // Keep connection alive and detect disconnection
        public async Task HandleConnectionAsync(
            Guid clientId,
            WebSocket socket,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open &&
                       !cancellationToken.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Request cancelled
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
            finally
            {
                RemoveClient(clientId);

                if (socket.State == WebSocketState.Open ||
                    socket.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Connection closed",
                            CancellationToken.None);
                    }
                    catch (WebSocketException)
                    {
                        // Connection already closed
                    }
                }

                socket.Dispose();
            }
        }
    }
}