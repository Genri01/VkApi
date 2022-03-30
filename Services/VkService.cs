using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkApi.Models;
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
        private readonly VkNet.VkApi _api;

        public VkService(ILogger<VkService> logger)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();

            _api = new VkNet.VkApi(services);
        }

        public async Task<string> GetVkSettings(string _token)
        {
            var token = "6986daabc75dbd577df64039501c1b9347b336547bf508591494d8487c851fb3f071797a56af53b72b1bb";
            await Authorize(_token);

            return "Settings Ok";
        }

        public async Task SendMessage(string _token, MessagesSendParams _messagesSendParams)
        {
            await Authorize(_token);
            await _api.Messages.SendAsync(new MessagesSendParams
            {
                PeerId = _messagesSendParams.PeerId,
                Message = _messagesSendParams.Message,
                RandomId = new Random().Next()
            });
        }

        public async Task AddFriends(string _token, AddFriendsModel _addFriendsModel)
        {
            await Authorize(_token);
            
            foreach (var userId in _addFriendsModel.UserIds)
            {
                await _api.Friends.AddAsync(userId, _addFriendsModel.WelcomeMessage, false);
            }
        }


        public async Task<VkCollection<User>> FilterSuggestionsFriends(string _token, FriendsFilter friendsFilter)
        {
            await Authorize(_token);

            return await _api.Friends.GetSuggestionsAsync(friendsFilter);
        }

        public async Task<VkCollection<User>> GetMembersFromGroup(string _token, GroupsGetMembersParams _groupsGetMembersParams, string groupName = null)
        {
            await Authorize(_token);

            string groupId = null;
            if (!string.IsNullOrEmpty(groupName))
            {
                var group = await _api.Utils.ResolveScreenNameAsync(groupName);
                groupId = group.Id.ToString();
            }

            return await _api.Groups.GetMembersAsync(new GroupsGetMembersParams()
                { GroupId = groupId ?? _groupsGetMembersParams.GroupId, Fields = UsersFields.All });
        }

        private async Task Authorize(string _token)
        {
            _token = "6986daabc75dbd577df64039501c1b9347b336547bf508591494d8487c851fb3f071797a56af53b72b1bb";
            await _api.AuthorizeAsync(new ApiAuthParams()
            {
                AccessToken = _token
            });
        }
    }
}
