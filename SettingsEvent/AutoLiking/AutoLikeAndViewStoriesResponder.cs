using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkApi.SettingsEvent.AutoLiking
{
    public class AutoLikeAndViewStoriesResponder
    {
        public int CountFriends { get; set; } // количество друзей, которым поставить лайк
        public bool SetRandomLike { get; set; } // ставить рандомный лайк
    }
}
