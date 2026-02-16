namespace TcpServerApp1.DTO
{
    public class LiveMessageDto
    {
        public string ClientEndpoint { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime ReceivedAt { get; set; }
        public int ThreadId { get; set; }
        public Boolean IsProcessed { get; set; }
        public string Priority { get; set; }
    }
}