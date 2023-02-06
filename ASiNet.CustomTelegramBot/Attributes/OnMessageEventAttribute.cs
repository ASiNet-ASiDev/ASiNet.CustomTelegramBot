using ASiNet.CustomTelegramBot.Enums;
using Telegram.Bot.Types.Enums;

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
