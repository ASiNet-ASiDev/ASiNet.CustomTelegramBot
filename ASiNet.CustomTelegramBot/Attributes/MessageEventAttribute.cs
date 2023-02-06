using ASiNet.CustomTelegramBot.Enums;
using Telegram.Bot.Types.Enums;

namespace ASiNet.CustomTelegramBot.Attributes;
[AttributeUsage(AttributeTargets.Method)]
public class MessageEventAttribute : Attribute
{
    public MessageEventAttribute(MessageTypeFlags typesAccepted = MessageTypeFlags.AllTypes)
    {
        AcceptedMessageTypes = typesAccepted;
    }

    public MessageTypeFlags AcceptedMessageTypes;
}
