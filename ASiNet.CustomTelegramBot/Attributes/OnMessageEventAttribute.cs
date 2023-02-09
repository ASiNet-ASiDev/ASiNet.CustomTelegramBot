using ASiNet.CustomTelegramBot.Enums;

namespace ASiNet.CustomTelegramBot.Attributes;
[AttributeUsage(AttributeTargets.Method)]
public class OnMessageEventAttribute : Attribute
{
    public OnMessageEventAttribute(MessageTypeFlags msgTypesFilter = MessageTypeFlags.AllTypes)
    {
        MessageTypesFilter = msgTypesFilter;
    }

    public MessageTypeFlags MessageTypesFilter;
}
