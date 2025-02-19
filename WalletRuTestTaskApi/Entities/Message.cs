namespace WalletRuTestTaskApi.Entities;

public class Message
{
    public string Content;
    public DateTime DateTime;

    public Message()
    {
    }

    public Message(string content, DateTime dateTime)
    {
        Content = content;
        DateTime = dateTime;
    }
}