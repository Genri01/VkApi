using System.Collections.Generic;

namespace VkApi.SettingsEvent.AutoAddedFriends
{
    public class AddSuggestFriendsModel
    {
        public string AcсountToken { get; set; } // токены пользователей
        public int Delay { get; set; } // задержка при выполнении задания
        public int RequestCount { get; set; } // количество заявок в друзья
        public string welcomeMessage { get; set; }
        public bool SetLikeToWall { get; set; }
        public bool SetLikeToProfilePhoto { get; set; }
    }
}
