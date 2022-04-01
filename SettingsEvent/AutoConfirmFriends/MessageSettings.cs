using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkApi.SettingsEvent.AutoResponder
{
    public class MessageSettings
    {
        // перенести в Enum
        public ConversationType ConversationTypeEvent { get; set; } // условие написания сообщения
        public List<string> TextMessages { get; set; } // список приветственных сообщений, которые будут отправляться рандомно
    }

    public enum ConversationType
    {
        ConversationIsEmpty = 1, // если переписка пустая
        ConversationIsEmptyOrNoAnwerFromMe = 2, // если переписка пустая или нет ответа от меня
        AnyCase = 3 //отправлять в любом случае
    }
}
