using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using WalletRuTestTaskApi.Entities;

namespace WalletRuTestTaskApi.Services;

public class WebSocketsHandler
{
    private readonly ILogger<WebSocketsHandler> _logger;
    private readonly List<WebSocket> _sockets = new();

    public WebSocketsHandler(ILogger<WebSocketsHandler> logger)
    {
        _logger = logger;
    }
    
    public async Task HandleConnectionAsync(WebSocket webSocket)
    {
        _sockets.Add(webSocket);
        _logger.LogInformation("New socket connection");
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                _sockets.Remove(webSocket);
                _logger.LogInformation("Socket connection closed");
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                break;
            }
        }
    }

    public async Task BroadcastMessageAsync(Message message)
    {
        string jsonMessage = JsonConvert.SerializeObject(message);
        byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

        foreach (var socket in _sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}