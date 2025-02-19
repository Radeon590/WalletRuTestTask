using System.Text;
using Newtonsoft.Json;

HttpClient httpClient = new();
const string apiUrl = "http://localhost:5235/api/Messaging";
int messagesCounter = 0;
        
while (true)
{
    string messageContent = $"{messagesCounter}";
    messagesCounter++;
    
    var uri = $"{apiUrl}?content={messageContent}";

    var response = await httpClient.PostAsync(uri, null);
    Console.WriteLine($"Message with content {messageContent} was sent with status code {response.StatusCode}");
    await Task.Delay(10000);
}