namespace WalletRuTestTask.Api.Entities;

public class Message
{
    public string Content { get; set; }
    public DateTime DateTime { get; set; }

    public Message()
    {
    }

    public Message(string content, DateTime dateTime)
    {
        Content = content;
        DateTime = dateTime;
    }
}