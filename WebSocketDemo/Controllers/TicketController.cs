using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebSocketDemo.Models;
using WebSocketDemo.Services;

namespace WebSocketDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly WebSocketDemo.Services.WebSocketManager _webSocketManager;

        public TicketController(WebSocketDemo.Services.WebSocketManager webSocketManager)
        {
            _webSocketManager = webSocketManager;
        }

        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(
            [FromBody] TicketNotification request)
        {
            // Normally, update ticket status in SQL Server here.

            request.UpdatedAt = DateTime.UtcNow;

            // Convert object into JSON
            var jsonMessage = JsonSerializer.Serialize(request);

            // Broadcast to all connected clients
            await _webSocketManager.BroadcastAsync(jsonMessage);

            return Ok(new
            {
                Success = true,
                Message = "Ticket updated and notification sent."
            });
        }
    }
}