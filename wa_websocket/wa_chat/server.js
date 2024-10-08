
const WebSocket = require('ws');
const wss = new WebSocket.Server({ port: 8000 });

// Broadcast function to send messages to all connected clients
function broadcastMessage(data, sender) {
    wss.clients.forEach(client => {
        if (client !== sender && client.readyState === WebSocket.OPEN) {
            client.send(data.toString());
        }
    });
}

// When a client connects
wss.on('connection', (ws) => {
    console.log('A new client connected!');

    // Listen for messages from the client
    ws.on('message', (message) => {
        console.log(`Received message: ${message}`);
        broadcastMessage(message, ws);
    });

    // Handle client disconnection
    ws.on('close', () => {
        console.log('Client disconnected');
    });
});

console.log('WebSocket server is running on ws://localhost:8000');
