In chrome console 
const ws = new WebSocket("wss://localhost:7107/ws");

ws.onopen = () => {
    console.log("WebSocket Connected Successfully!");
};

ws.onmessage = (event) => {
    console.log("Notification Received:", JSON.parse(event.data));
};

ws.onerror = (error) => {
    console.log("WebSocket Error:", error);
};

ws.onclose = () => {
    console.log("WebSocket Disconnected!");
};

Url: https://localhost:7107/api/Ticket/UpdateStatus
========================
{
    "ticketNumber": "WC-10001",
    "status": "IN_PROGRESS",
    "message": "Ticket assigned to Agent Amit"
}
