using System.Collections.Generic;
using System.Threading.Tasks;
using VkApi.SettingsEvent.AutoAddedFriends;
using VkApi.SettingsEvent.AutoLikingFriends;
using VkApi.SettingsEvent.AutoResponder;
using VkApi.SettingsEvent.SuggestFriendsFilter;
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
        Task AutoResponderFriends(AutoFriendsResponderModel _autoFriendsResponderModel);
        Task AutoLikingFriendsOrGroups(AutoLikingFriends _autoLikingFriends);

        Task<IEnumerable<UserModel>> FilterSuggestionsFriends(string _token, SuggestFriendsFilter _friendsFilter);

        Task<VkCollection<User>> GetGroups(string _token, GroupsGetMembersParams _groupsGetMembersParams,
            string groupName = null);
    }
}
