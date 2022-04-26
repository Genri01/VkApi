using System.Collections.Generic;
using System.Threading.Tasks;
using VkApi.SettingsEvent.AutoAddedFriends;
using VkApi.SettingsEvent.AutoLikingFriends;
using VkApi.SettingsEvent.AutoResponder;
using VkApi.SettingsEvent.SuggestFriendsFilter;
using VkNet.Model;
using VkNet.Utils;

namespace VkApi.Services
{
    public interface IVkService
    {
        Task AddSuggestFriends(string _token, AddSuggestFriendsModel _addSuggestFriendsModel);
        Task AutoResponderFriends(string _token, AutoFriendsResponderModel _autoFriendsResponderModel);
        Task AutoLikingFriendsOrGroups(string _token, AutoLikingFriends _autoLikingFriends);
        Task<IEnumerable<UserModel>> FilterSuggestionsFriends(string _token, SuggestFriendsFilter _friendsFilter);
        Task<VkCollection<User>> GetMembersFromGroup(string _token, string groupName);
    }
}
