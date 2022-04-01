using System.Collections.Generic;
using System.Reflection;

namespace VkApi.SettingsEvent.AutoLikingFriends
{
    public class AutoLikingFriends
    {
        public List<string> AcountTokens { get; set; } // токены пользователей
        public int Delay { get; set; }
        public int RequestCount { get; set; }
        public List<long> UserIds { get; set; }
        public List<long> GroupIds { get; set; }
        public bool SetLikeToWall { get; set; }
        public bool SetLikeToProfilePhoto { get; set; }
    }
}
