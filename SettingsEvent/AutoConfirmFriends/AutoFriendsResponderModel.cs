using System.Collections.Generic;

namespace VkApi.SettingsEvent.AutoResponder
{
    public class AutoFriendsResponderModel
    {
        public string AcountToken { get; set; } // токены пользователей
        public int Delay { get; set; } // задержка при выполнении задания
        public AutoResponderEventType AutoResponderEventType { get; set; } // тип выполнения события
        public int WelcomeCount { get; set; } // количество приветствий 
        public MessageSettings MessageSettings { get; set; } // настройка отправки текста сообщений
        public PhotoOrVideoSettings PhotoOrVideoSettings { get; set; } // настройка отправки фото или видео
        public AudioSettings AudioSettings { get; set; } // настройка отправки аудио из файла
        public List<string> UserNamesOrIds { get; set; } // список пользователей
        public List<string> GroupNamesOrIds { get; set; } // список пользователей
        public bool AddToFriends { get; set; }
        public bool SetLikeToWall { get; set; }
        public bool SetLikeToProfile { get; set; }
    }

    public enum AutoResponderEventType
    {
        СonfirmedFriendsRequests = 1, // подтверждённые заявки в друзья
        IncomingFriendsRequests = 2, // входящие заявки в друзья
        SpecificUsers = 3, // по конкретному списку пользователей
        SpecificGroups = 4 // по конкретному списку сообществ
    }
}
