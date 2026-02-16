namespace TcpServerApp1.DTO
{
    public class AlertItem
    {
        public string ClientEndpoint { get; set; } = "";
        public string Content { get; set; } = "";
        public string Priority { get; set; } = "";
        public DateTime RecievedAt { get; set; } = DateTime.Now;
        public Boolean isProcessed { get; set; } = true;
    }
}