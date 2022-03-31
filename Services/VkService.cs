using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkApi.SettingsEvent.AutoAddedFriends;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VkApi.Services
{
    public class VkService : IVkService
    {

        private readonly ILogger<VkService> _logger;
        
        public VkService(ILogger<VkService> logger)
        {
        }

        public async Task<string> GetVkSettings(string _token)
        {
            var token = "6986daabc75dbd577df64039501c1b9347b336547bf508591494d8487c851fb3f071797a56af53b72b1bb";
            var api = await Authorize(_token);

            return "Settings Ok";
        }

        public async Task SendMessage(string _token, MessagesSendParams _messagesSendParams)
        {
            var api = await Authorize(_token);
            await api.Messages.SendAsync(new MessagesSendParams
            {
                PeerId = _messagesSendParams.PeerId,
                Message = _messagesSendParams.Message,
                RandomId = new Random().Next()
            });
        }

        public async Task AddSuggestFriends(AddSuggestFriendsModel _addSuggestFriendsModel)
        {
            foreach (var token in _addSuggestFriendsModel.AcountTokens)
            {
                var api = await Authorize(token);
                
                var suggestFriends = await api.Friends.GetSuggestionsAsync();

                for (int i = 0; i < _addSuggestFriendsModel.RequestCount; i++)
                {
                    await api.Friends.AddAsync(suggestFriends.ElementAt(i).Id, _addSuggestFriendsModel.wlecomeMessage, false);
                }
            }
        }


        public async Task<VkCollection<User>> FilterSuggestionsFriends(string _token, FriendsFilter friendsFilter)
        {
            var api = await Authorize(_token);

            return await api.Friends.GetSuggestionsAsync(friendsFilter);
        }

        public Task<VkCollection<User>> GetGroups(string _token, GroupsGetMembersParams _groupsGetMembersParams, string groupName = null)
        {
            throw new NotImplementedException();
        }

        public async Task<VkCollection<User>> GetMembersFromGroup(string _token, GroupsGetMembersParams _groupsGetMembersParams, string groupName = null)
        {
            var api = await Authorize(_token);

            string groupId = null;
            if (!string.IsNullOrEmpty(groupName))
            {
                var group = await api.Utils.ResolveScreenNameAsync(groupName);
                groupId = group.Id.ToString();
            }

            return await api.Groups.GetMembersAsync(new GroupsGetMembersParams()
                { GroupId = groupId ?? _groupsGetMembersParams.GroupId, Fields = UsersFields.All });
        }

        private async Task<VkNet.VkApi> Authorize(string _token)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();

            var api = new VkNet.VkApi(services);

            _token = "6986daabc75dbd577df64039501c1b9347b336547bf508591494d8487c851fb3f071797a56af53b72b1bb";
            await api.AuthorizeAsync(new ApiAuthParams()
            {
                AccessToken = _token
            });
            return api;
        }
    }
}
