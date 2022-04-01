using System.Collections.Generic;

namespace VkApi.SettingsEvent.AutoAddedFriends
{
    public class AddSuggestFriendsModel
    {
        public List<string> AcountTokens { get; set; } // токены пользователей
        public int RequestCount { get; set; } // количество заявок в друзья
        public string welcomeMessage { get; set; }
        public bool SetLikeToWall { get; set; }
        public bool SetLikeToProfilePhoto { get; set; }
    }
}
