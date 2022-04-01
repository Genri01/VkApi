using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkApi.SettingsEvent.AutoAddedFriends;
using VkApi.SettingsEvent.AutoResponder;
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

                var t = suggestFriends.Count;

                _addSuggestFriendsModel.RequestCount =
                    (suggestFriends.Count < _addSuggestFriendsModel.RequestCount)
                        ? suggestFriends.Count
                        : _addSuggestFriendsModel.RequestCount;

                for (var i = 0; i < _addSuggestFriendsModel.RequestCount; i++)
                {
                    var userId = suggestFriends.ElementAt(i).Id;
                    await api.Friends.AddAsync(suggestFriends.ElementAt(i).Id, _addSuggestFriendsModel.welcomeMessage, false);

                    if (_addSuggestFriendsModel.SetLikeToProfilePhoto)
                        await SetLikeToProfile(api, userId);

                    if (_addSuggestFriendsModel.SetLikeToWall)
                        await SetLikeToWall(api, userId);
                }
            }
        }

        public async Task AutoResponderFriends(AutoFriendsResponderModel _autoFriendsResponderModel)
        {
            foreach (var token in _autoFriendsResponderModel.AcountTokens)
            {
                var api = await Authorize(token);

                switch (_autoFriendsResponderModel.AutoResponderEventType)
                {
                    case AutoResponderEventType.СonfirmedFriendsRequests:
                        await ConfirmFriendsWorker(api, _autoFriendsResponderModel);
                        break;
                    case AutoResponderEventType.IncomingFriendsRequests:
                        await IncomingFriendsWorker(api, _autoFriendsResponderModel);
                        break;
                    case AutoResponderEventType.SpecificUsers:
                        await SpecificUsersWorker(api, _autoFriendsResponderModel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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

        private async Task ConfirmFriendsWorker(VkNet.VkApi api, AutoFriendsResponderModel _autoFriendsResponderModel)
        {
            var confirmFriends = await api.Friends.GetAsync(new FriendsGetParams());

            var userIds = new List<long>(confirmFriends.Select(x => x.Id));
            userIds = await FilterUsersByConversation(api, _autoFriendsResponderModel.MessageSettings.ConversationTypeEvent, userIds);

            if (_autoFriendsResponderModel.MessageSettings != null && _autoFriendsResponderModel.MessageSettings.TextMessages != null && _autoFriendsResponderModel.MessageSettings.TextMessages.Any())
            {
                await SendMessages(api, _autoFriendsResponderModel.MessageSettings.TextMessages,
                    _autoFriendsResponderModel.WelcomeCount, userIds);
            }

            var likeProfileAndLikeWallCount = userIds.Count <= _autoFriendsResponderModel.WelcomeCount ? userIds.Count : _autoFriendsResponderModel.WelcomeCount;

            if (_autoFriendsResponderModel.SetLikeToProfile)
            {
                for (int i = 0; i < likeProfileAndLikeWallCount; i++)
                {
                    await SetLikeToProfile(api, confirmFriends.ElementAt(i).Id);
                }
            }

            if (_autoFriendsResponderModel.SetLikeToWall)
            {
                for (int i = 0; i < likeProfileAndLikeWallCount; i++)
                {
                    await SetLikeToWall(api, confirmFriends.ElementAt(i).Id);
                }
            }
        }

        private async Task IncomingFriendsWorker(VkNet.VkApi api, AutoFriendsResponderModel _autoFriendsResponderModel)
        {
            var requestFriends = await api.Friends.GetRequestsAsync(new FriendsGetRequestsParams
            {
                NeedViewed = true
            });

            var userIds = new List<long>(requestFriends.Items);
            userIds = await FilterUsersByConversation(api, _autoFriendsResponderModel.MessageSettings.ConversationTypeEvent, userIds);

            if (_autoFriendsResponderModel.MessageSettings != null && _autoFriendsResponderModel.MessageSettings.TextMessages != null && _autoFriendsResponderModel.MessageSettings.TextMessages.Any())
            {
                await SendMessages(api, _autoFriendsResponderModel.MessageSettings.TextMessages,
                    _autoFriendsResponderModel.WelcomeCount, userIds);
            }

            var likeProfileAndLikeWallCountAndAddToFriendCount = userIds.Count <= _autoFriendsResponderModel.WelcomeCount ? userIds.Count : _autoFriendsResponderModel.WelcomeCount;

            if (_autoFriendsResponderModel.SetLikeToProfile)
            {
                for (int i = 0; i < likeProfileAndLikeWallCountAndAddToFriendCount; i ++)
                {
                    await SetLikeToProfile(api, requestFriends.Items.ElementAt(i));
                }
            }

            if (_autoFriendsResponderModel.SetLikeToWall)
            {
                for (int i = 0; i < likeProfileAndLikeWallCountAndAddToFriendCount; i++)
                {
                    await SetLikeToWall(api, requestFriends.Items.ElementAt(i));
                }
            }

            if (_autoFriendsResponderModel.AddToFriends)
            {
                for (int i = 0; i < likeProfileAndLikeWallCountAndAddToFriendCount; i++)
                {
                    await api.Friends.AddAsync(requestFriends.Items.ElementAt(i), "", false);
                }
            }
        }

        private async Task SpecificUsersWorker(VkNet.VkApi api, AutoFriendsResponderModel _autoFriendsResponderModel)
        {
            var specificUsers = await api.Users.GetAsync(_autoFriendsResponderModel.UserIds);

            var userIds = new List<long>(specificUsers.Select(x => x.Id));
            userIds = await FilterUsersByConversation(api, _autoFriendsResponderModel.MessageSettings.ConversationTypeEvent, userIds);

            if (_autoFriendsResponderModel.MessageSettings != null && _autoFriendsResponderModel.MessageSettings.TextMessages != null && _autoFriendsResponderModel.MessageSettings.TextMessages.Any())
            {
                await SendMessages(api, _autoFriendsResponderModel.MessageSettings.TextMessages,
                    userIds.Count, userIds);
            }

            if (_autoFriendsResponderModel.SetLikeToProfile)
            {
                foreach (var user in specificUsers)
                {
                    await SetLikeToProfile(api, user.Id);
                }
            }

            if (_autoFriendsResponderModel.SetLikeToWall)
            {
                foreach (var user in specificUsers)
                {
                    await SetLikeToWall(api, user.Id);
                }
            }

            if (_autoFriendsResponderModel.AddToFriends)
            {
                foreach (var user in specificUsers)
                {
                    await api.Friends.AddAsync(user.Id, "", false);
                }
            }
        }

        private async Task<List<long>> FilterUsersByConversation(VkNet.VkApi api, ConversationType conversationType, List<long> userIds)
        {
            var updateUserIdList = new List<long>();

            switch (conversationType)
            {
                case ConversationType.ConversationIsEmpty:
                {
                    foreach (var userId in userIds)
                    {
                        var history = await api.Messages.GetHistoryAsync(new MessagesGetHistoryParams()
                        {
                            UserId = userId,
                        });

                        if (history?.Messages is null || !history.Messages.Any())
                            updateUserIdList.Add(userId);
                    }

                    return updateUserIdList;
                }
                case ConversationType.ConversationIsEmptyOrNoAnwerFromMe:
                {
                    foreach (var userId in userIds)
                    {
                        var history = await api.Messages.GetHistoryAsync(new MessagesGetHistoryParams()
                        {
                            UserId = userId,
                        });

                        if (history?.Messages is null || !history.Messages.Any())
                        {
                            updateUserIdList.Add(userId);
                        }
                        else if (history.Messages.Any())
                        {
                            var outgoingMessages = history.Messages.Where(x => x.Out == false);

                            if (outgoingMessages.Any())
                                updateUserIdList.Add(userId);
                        }
                    }

                    return updateUserIdList;
                }
                case ConversationType.AnyCase:
                    return userIds;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conversationType), conversationType, null);
            }
        }

        private async Task SetLikeToProfile(VkNet.VkApi api, long userId)
        {
            var photos = await api.Photo.GetAsync(new PhotoGetParams
            {
                OwnerId = userId,
                AlbumId = PhotoAlbumType.Profile,
            });

            if (photos != null && photos.Any())
            {
                var photoId = (long)photos.First().Id;
                await api.Likes.AddAsync(new LikesAddParams
                {
                    Type = LikeObjectType.Photo,
                    OwnerId = userId,
                    ItemId = photoId
                });
            }
        }

        private async Task SetLikeToWall(VkNet.VkApi api, long userId)
        {
            var post = await api.Wall.GetAsync(new WallGetParams
            {
                OwnerId = userId,
                Count = 1
            });

            if (post != null && post.WallPosts.Any())
            {
                var postId = (long)post.WallPosts.First().Id;
                await api.Likes.AddAsync(new LikesAddParams
                {
                    Type = LikeObjectType.Post,
                    OwnerId = userId,
                    ItemId = postId
                });
            }
        }

        private async Task SendMessages(VkNet.VkApi api, List<string> welcomeMessages, int welcomeCount, List<long> userIds)
        {
            welcomeCount = userIds.Count < welcomeCount ? userIds.Count : welcomeCount;

            for (int i = 0; i < welcomeCount; i++)
            {
                api.Messages.Send(new MessagesSendParams()
                {
                    UserId = userIds.ElementAt(i),
                    Message = welcomeMessages.ElementAt(new Random().Next(0, welcomeMessages.Count)),
                    RandomId = new Random().Next()
                });
            }
        }
    }
}
