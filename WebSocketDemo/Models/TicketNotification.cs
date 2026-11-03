namespace WebSocketDemo.Models
{
    public class TicketNotification
    {
        public string TicketNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
