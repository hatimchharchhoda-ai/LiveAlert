using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace TcpServerApp1.RealTime
{
    public class LiveDataBroadcaster
    {
        private readonly Dictionary<string, WebSocket> _sockets = new();
        private readonly object _lock = new();

        public void AddSocket(string id, WebSocket socket)
        {
            lock (_lock)
            {
                _sockets[id] = socket;
            }
        }

        public void RemoveSocket(string id)
        {
            lock (_lock)
            {
                _sockets.Remove(id);
            }
        }

        public async Task BroadcastAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            var buffer = Encoding.UTF8.GetBytes(json);

            List<WebSocket> sockets;
            lock (_lock)
            {
                sockets = _sockets.Values.ToList();
            }

            foreach (var socket in sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(buffer),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
            }
        }
    }
}