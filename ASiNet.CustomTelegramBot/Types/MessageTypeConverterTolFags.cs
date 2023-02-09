using ASiNet.CustomTelegramBot.Enums;
using Telegram.Bot.Types.Enums;

namespace ASiNet.CustomTelegramBot.Types;
internal static class MessageTypeConverterTolFags
{
    public static MessageTypeFlags GetFlags(MessageType type)
    {
        var name = Enum.GetName(type);
        if (name is null)
            throw new ArgumentException("MessageType value not found!");
        var result = Enum.Parse<MessageTypeFlags>(name, true);
        return result;
    }

}
