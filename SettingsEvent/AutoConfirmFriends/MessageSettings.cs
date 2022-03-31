using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkApi.SettingsEvent.AutoResponder
{
    public class MessageSettings
    {
        // перенести в Enum
        public bool DialogIsEmpty { get; set; } //если переписка пустая
        public bool DialogIsEmptyOrNoAnwerFromMe { get; set; } //если переписка пустая или нет ответа от меня
        public bool AnyCase { get; set; } //писать в любом случае
        public List<string> TextMessages { get; set; } //список приветсвенных сообщений, которые будут отправляться рандомно
    }
}
