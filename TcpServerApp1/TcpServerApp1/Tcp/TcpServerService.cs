using System.Net;
using System.Net.Sockets;
using System.Text;
using TcpServerApp1.Alert_store;
using TcpServerApp1.DTO;
using TcpServerApp1.RealTime;

namespace TcpServerApp1.Tcp
{
    public class TcpServerService : BackgroundService
    {
        private readonly SemaphoreSlim _clientLimiter = new(4);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly LiveDataBroadcaster _broadcaster;
        private readonly AlertQueue _alertQueue;
        private TcpListener? _listener;

        public TcpServerService(
            IServiceScopeFactory scopeFactory,
            LiveDataBroadcaster broadcaster,
            AlertQueue alertQueue)
        {
            _scopeFactory = scopeFactory;
            _broadcaster = broadcaster;
            _alertQueue = alertQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener = new TcpListener(IPAddress.Any, 5000);
            _listener.Start();

            Console.WriteLine("TCP Server started on port 5000");

            // Processing the Alert
            _ = Task.Run(() => ProcessAlerts(stoppingToken), stoppingToken);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(stoppingToken);

                    _ = Task.Run(async () =>
                    {
                        await _clientLimiter.WaitAsync(stoppingToken);
                        try
                        {
                            await HandleClientAsync(client, stoppingToken);
                        }
                        finally
                        {
                            _clientLimiter.Release();
                        }
                    }, stoppingToken);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _listener?.Stop();
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
        {
            var threadId = Environment.CurrentManagedThreadId;
            Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");

            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var message = await reader.ReadLineAsync();
                    if (message == null) break;

                    var endpoint = client.Client.RemoteEndPoint!.ToString();

                    // Priority Queue Add
                    var priority = message.Split('|')[2]
                                              .Trim();
                    Console.WriteLine(priority);

                    var alert = new AlertItem
                    {
                        ClientEndpoint = endpoint,
                        Content = message,
                        Priority = priority,
                        RecievedAt = DateTime.Now,
                    };

                    switch(priority)
                    {
                        case "Critical":
                            _alertQueue.Critical.Enqueue(alert);
                            break;
                        case "High":
                            _alertQueue.High.Enqueue(alert);
                            break;
                        default:
                            _alertQueue.Low.Enqueue(alert);
                            break;
                    }

                    var entity = new
                    {
                        ClientEndpoint = endpoint,
                        Content = message,
                        ReceivedAt = DateTime.Now,
                        ThreadId = Environment.CurrentManagedThreadId
                    };

                    await _broadcaster.BroadcastAsync(new LiveMessageDto
                    {
                        ClientEndpoint = entity.ClientEndpoint,
                        Content = entity.Content,
                        ReceivedAt = entity.ReceivedAt,
                        ThreadId = entity.ThreadId,
                        IsProcessed = false,
                        Priority = "",
                    });
                }
            }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                client.Close();
                Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}");
            }
        }


        // Processing every Alerts
        private async Task ProcessAlerts(CancellationToken token)
        {
            //Console.WriteLine("1-------------------1");
            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine("2-------------------2");
                AlertItem? alert = null;
                if (_alertQueue.Critical.TryDequeue(out alert) ||
                    _alertQueue.High.TryDequeue(out alert) ||
                    _alertQueue.Low.TryDequeue(out alert))
                {
                    //Console.WriteLine("3-------------------3");
                    await _broadcaster.BroadcastAsync(new LiveMessageDto
                    {
                        ClientEndpoint = alert.ClientEndpoint,
                        Content = alert.Content,
                        Priority = alert.Priority,
                        ReceivedAt = alert.RecievedAt,
                        IsProcessed = alert.isProcessed,
                    });
                    //Console.WriteLine("4-------------------4");
                    await Task.Delay(3000, token);
                }
                else
                {
                    //Console.WriteLine("-------------------");
                    await Task.Delay(500, token);
                }
            }
        }
    }
}