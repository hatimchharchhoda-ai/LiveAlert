using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;

class Program
{
    static async Task Main()
    {
        TcpClient? client = null;
        var clientId = Guid.NewGuid().ToString()[..8];
        int intervalMs = 2000;

        try
        {
            client = new TcpClient();
            Console.WriteLine($"[{clientId}] Connecting to server...");
            await client.ConnectAsync("127.0.0.1", 5000);
            Console.WriteLine($"[{clientId}] Connected");

            // Get local client port
            int localPort = ((IPEndPoint)client.Client.LocalEndPoint!).Port;
            Console.WriteLine($"[{clientId}] Local Port: {localPort}");

            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8)
            {
                AutoFlush = true
            };

            int counter = 0;
            Random random = new Random();

            while (true)
            {
                counter++;
                int threadId = Thread.CurrentThread.ManagedThreadId;
                string payload= " ";
                string filePath = "C:\\Users\\hatim.chharchhoda\\LiveAlert\\TcpClientApp1\\sensor-data.txt";

                if (File.Exists(filePath))
                {
                    using var fileReader = new StreamReader(filePath);
                    Console.WriteLine($"[{clientId}] Reading from file: {filePath}");

                    string? line;
                    while ((line = await fileReader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        payload = $"Alert | {line}";

                        await writer.WriteLineAsync(payload);
                        Console.WriteLine($"[{clientId}] [SENT] {payload}");

                        await Task.Delay(intervalMs);
                    }
                    Console.WriteLine($"[{clientId}] File completed. Disconnecting...");
                    break;   
                }
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client error: {ex.Message}");
        }
        finally
        {
            client?.Close();
            Console.WriteLine("Client disconnected");
        }
    }
}