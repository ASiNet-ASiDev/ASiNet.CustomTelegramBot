namespace ASiNet.CustomTelegramBot.Attributes;
[AttributeUsage(AttributeTargets.Method)]
public class OnCommandEventAttribute : Attribute
{
    public OnCommandEventAttribute(string command)
    {
        Command = command;
    }

    public string Command { get; set; }
}
