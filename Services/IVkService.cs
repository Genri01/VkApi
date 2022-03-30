using System.Threading.Tasks;
using VkApi.Models;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VkApi.Services
{
    public interface IVkService
    {
        Task<string> GetVkSettings(string _token);
        Task SendMessage(string _token, MessagesSendParams _messagesSendParams);
        Task AddFriends(string _token, AddFriendsModel _addFriendsModel);
        Task<VkCollection<User>> FilterSuggestionsFriends(string _token, FriendsFilter _friendsFilter);

        Task<VkCollection<User>> GetGroups(string _token, GroupsGetMembersParams _groupsGetMembersParams,
            string groupName = null);
    }
}
