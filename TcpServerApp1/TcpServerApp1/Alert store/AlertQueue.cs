using System.Collections.Concurrent;
using TcpServerApp1.DTO;

namespace TcpServerApp1.Alert_store
{
    public class AlertQueue
    {
        public ConcurrentQueue<AlertItem> Critical = new();
        public ConcurrentQueue<AlertItem> High = new();
        public ConcurrentQueue<AlertItem> Low = new();
    }
}