using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkApi.Models
{
    public class AddFriendsModel
    {
        public long[] UserIds { get; set; }
        public string WelcomeMessage { get; set; }
    }
}
