using System.Threading.Tasks;
using VkApi.SettingsEvent.AutoAddedFriends;
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
        Task AddSuggestFriends(AddSuggestFriendsModel _addSuggestFriendsModel);
        Task<VkCollection<User>> FilterSuggestionsFriends(string _token, FriendsFilter _friendsFilter);

        Task<VkCollection<User>> GetGroups(string _token, GroupsGetMembersParams _groupsGetMembersParams,
            string groupName = null);
    }
}
