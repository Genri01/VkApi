using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Model.Attachments;

namespace VkApi.SettingsEvent.AutoResponder
{
    public class AutoСonfirmFriendsResponder
    {
        // автоответчик на подтверждённые заявки в друзья
        public List<string> AcountTokens { get; set; } // токены пользователей
        
        public bool IsConfirm { get; set; } // подтверждённые заявки в друзья
        public bool IsIncoming { get; set; } // входящие заявки в друзья
        public bool AutoSender { get; set; } // по списку пользователей
        

        public int CountGreeting { get; set; } // количество приветствий 
        public MessageSettings MessageSettings { get; set; } // настройка отправки текста сообщений
        public PhotoOrVideoSettings PhotoOrVideoSettings { get; set; } // настройка отправки фото или видео
        public AudioSettings AudioSettings { get; set; } // настройка отправки аудио из файла
        public List<string> userIds { get; set; }
        public bool AddToFriends { get; set; }
        public bool SetLikeToWall { get; set; }
        public bool SetLikeToAvatar { get; set; }

    }
}
