using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkApi.SettingsEvent.AutoAddedFriends;
using VkApi.SettingsEvent.AutoLikingFriends;
using VkApi.SettingsEvent.AutoResponder;
using VkApi.SettingsEvent.SuggestFriendsFilter;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VkApi.Services
{
    public class VkService : IVkService
    {
        private readonly ILogger<VkService> _logger;

        public VkService(ILogger<VkService> logger)
        {
            _logger = logger;
        }

        public async Task AddSuggestFriends(string _token, AddSuggestFriendsModel _addSuggestFriendsModel)
        {
            var api = await Authorize(_token);

            var suggestFriends = await api.Friends.GetSuggestionsAsync();

            _addSuggestFriendsModel.RequestCount =
                (suggestFriends.Count < _addSuggestFriendsModel.RequestCount)
                    ? suggestFriends.Count
                    : _addSuggestFriendsModel.RequestCount;

            for (var i = 0; i < _addSuggestFriendsModel.RequestCount; i++)
            {
                Thread.Sleep(_addSuggestFriendsModel.Delay * 1000);

                var userId = suggestFriends.ElementAt(i).Id;
                await api.Friends.AddAsync(suggestFriends.ElementAt(i).Id, _addSuggestFriendsModel.welcomeMessage,
                    false);

                if (_addSuggestFriendsModel.SetLikeToProfilePhoto)
                    await SetLikeToProfile(api, userId);

                if (_addSuggestFriendsModel.SetLikeToWall)
                    await SetLikeToWall(api, userId);
            }
        }

        public async Task AutoResponderFriends(string _token, AutoFriendsResponderModel _autoFriendsResponderModel)
        {
            var api = await Authorize(_token);

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
                case AutoResponderEventType.SpecificGroups:
                    await SpecificGroupsWorker(api, _autoFriendsResponderModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task AutoLikingFriendsOrGroups(string _token, AutoLikingFriends _autoLikingFriends)
        {
            var api = await Authorize(_token);

            var confirmFriends = await api.Friends.GetAsync(new FriendsGetParams());

            _autoLikingFriends.RequestCount =
                (confirmFriends.Count < _autoLikingFriends.RequestCount)
                    ? confirmFriends.Count
                    : _autoLikingFriends.RequestCount;

            for (var i = 0; i < _autoLikingFriends.RequestCount; i++)
            {
                Thread.Sleep(_autoLikingFriends.Delay * 1000);

                var userId = confirmFriends.ElementAt(i).Id;

                if (_autoLikingFriends.SetLikeToProfilePhoto)
                    await SetLikeToProfile(api, userId);

                if (_autoLikingFriends.SetLikeToWall)
                    await SetLikeToWall(api, userId);
            }
        }

        public async Task<IEnumerable<UserModel>> FilterSuggestionsFriends(string _token,
            SuggestFriendsFilter _friendsFilter)
        {
            var api = await Authorize(_token);

            var filter = _friendsFilter.SuggestFriendsFilterType switch
            {
                SuggestFriendsFilterType.Contacts => FriendsFilter.Contacts,
                SuggestFriendsFilterType.Mutual => FriendsFilter.Mutual,
                SuggestFriendsFilterType.MutualContacts => FriendsFilter.MutualContacts,
                _ => throw new ArgumentOutOfRangeException()
            };

            var users = await api.Friends.GetSuggestionsAsync(filter, _friendsFilter.Count);

            return users.Select(user => new UserModel
            { UserId = user.Id, FirstName = user.FirstName, LastName = user.LastName }).ToList();

        }

        public async Task<VkCollection<User>> GetMembersFromGroup(string _token, string groupName)
        {
            var api = await Authorize(_token);


            // Получить адрес сервера для загрузки.
            var uploadServer = api.Audio.GetUploadServer();

            // Загрузить файл.
            var wc = new WebClient();
            var responseFile =
                Encoding.ASCII.GetString(wc.UploadFile(uploadServer, @"D:\LRMonoPhase4.mp3"));

            // Сохранить загруженный файл
            var audio = await api.Audio.SaveAsync(responseFile);

            await api.Messages.SendAsync(new MessagesSendParams()
            {
                PeerId = 621118712,
                Attachments = new List<MediaAttachment>
                {
                    audio
                },
                Message = "Send Audio",
                RandomId = new Random().Next()
            });

            string groupId = null;
            if (!string.IsNullOrEmpty(groupName))
            {
                var group = await api.Utils.ResolveScreenNameAsync(groupName);
                groupId = group.Id.ToString();
            }

            return await api.Groups.GetMembersAsync(new GroupsGetMembersParams()
            { GroupId = groupId, Fields = UsersFields.FirstNameAcc });
        }

        private async Task<VkNet.VkApi> Authorize(string _token)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();

            var api = new VkNet.VkApi(services);

            try
            {
                await api.AuthorizeAsync(new ApiAuthParams()
                {
                    AccessToken = _token
                });
            }
            catch (Exception e)
            {
                _logger.LogError("Authorize is Failed", e.Message);
            }

            return api;
        }

        private async Task ConfirmFriendsWorker(VkNet.VkApi api, AutoFriendsResponderModel _autoFriendsResponderModel)
        {
            var myFriends = await api.Friends.GetAsync(new FriendsGetParams
            {
                Fields = ProfileFields.All
            });

            var confirmFriends = myFriends.Where(x => x?.IsClosed != true && x?.IsDeactivated != true).ToList();
            var userIds = new List<long>(confirmFriends.Select(x => x.Id));
            var specificUsersIdsWhereAccessWriteMessage = confirmFriends.Where(x => x.CanWritePrivateMessage).Select(x => x.Id).ToList();

            if (_autoFriendsResponderModel.MessageSettings != null &&
                _autoFriendsResponderModel.MessageSettings.TextMessages != null &&
                _autoFriendsResponderModel.MessageSettings.TextMessages.Any())
            {
                var userIdsForMessage = await FilterByConversation(api, _autoFriendsResponderModel.MessageSettings.ConversationTypeEvent,
                    specificUsersIdsWhereAccessWriteMessage);
                await SendMessages(api, _autoFriendsResponderModel.MessageSettings.TextMessages,
                    _autoFriendsResponderModel.WelcomeCount, userIdsForMessage, _autoFriendsResponderModel.Delay);
            }

            if (_autoFriendsResponderModel.PhotoOrVideoSettings != null &&
                _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath != null &&
                _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath.Any())
            {
                await SendMessageWithPhoto(api, _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath,
                    _autoFriendsResponderModel.PhotoOrVideoSettings.Messages, _autoFriendsResponderModel.WelcomeCount, userIds, _autoFriendsResponderModel.Delay);
            }

            if (_autoFriendsResponderModel.AudioSettings != null &&
                _autoFriendsResponderModel.AudioSettings.AudioFilesPath != null &&
                _autoFriendsResponderModel.AudioSettings.AudioFilesPath.Any())
            {
                await SendMessageWithAudio(api, _autoFriendsResponderModel.AudioSettings.AudioFilesPath,
                    _autoFriendsResponderModel.AudioSettings.Messages, _autoFriendsResponderModel.WelcomeCount, userIds, _autoFriendsResponderModel.Delay);
            }

            var likeProfileAndLikeWallCount = userIds.Count <= _autoFriendsResponderModel.WelcomeCount
                ? userIds.Count
                : _autoFriendsResponderModel.WelcomeCount;

            if (_autoFriendsResponderModel.SetLikeToProfile)
            {
                for (int i = 0; i < likeProfileAndLikeWallCount; i++)
                {
                    Thread.Sleep(_autoFriendsResponderModel.Delay * 1000);
                    await SetLikeToProfile(api, confirmFriends.ElementAt(i).Id);
                }
            }

            if (_autoFriendsResponderModel.SetLikeToWall)
            {
                for (int i = 0; i < likeProfileAndLikeWallCount; i++)
                {
                    Thread.Sleep(_autoFriendsResponderModel.Delay * 1000);
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

            var likeProfileAndLikeWallCountAndAddToFriendCount =
                userIds.Count <= _autoFriendsResponderModel.WelcomeCount
                    ? userIds.Count
                    : _autoFriendsResponderModel.WelcomeCount;

            if (_autoFriendsResponderModel.AddToFriends)
            {
                for (int i = 0; i < likeProfileAndLikeWallCountAndAddToFriendCount; i++)
                {
                    Thread.Sleep(_autoFriendsResponderModel.Delay * 1000);
                    await api.Friends.AddAsync(requestFriends.Items.ElementAt(i), "", false);
                }
            }

            if (_autoFriendsResponderModel.MessageSettings != null &&
                _autoFriendsResponderModel.MessageSettings.TextMessages != null &&
                _autoFriendsResponderModel.MessageSettings.TextMessages.Any())
            {
                var userIdsForMessage = await FilterByConversation(api, _autoFriendsResponderModel.MessageSettings.ConversationTypeEvent,
                    userIds);
                await SendMessages(api, _autoFriendsResponderModel.MessageSettings.TextMessages,
                    _autoFriendsResponderModel.WelcomeCount, userIdsForMessage, _autoFriendsResponderModel.Delay);
            }

            if (_autoFriendsResponderModel.PhotoOrVideoSettings != null &&
                _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath != null &&
                _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath.Any())
            {
                await SendMessageWithPhoto(api, _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath,
                    _autoFriendsResponderModel.PhotoOrVideoSettings.Messages, _autoFriendsResponderModel.WelcomeCount, userIds, _autoFriendsResponderModel.Delay);
            }

            if (_autoFriendsResponderModel.AudioSettings != null &&
                _autoFriendsResponderModel.AudioSettings.AudioFilesPath != null &&
                _autoFriendsResponderModel.AudioSettings.AudioFilesPath.Any())
            {
                await SendMessageWithAudio(api, _autoFriendsResponderModel.AudioSettings.AudioFilesPath,
                    _autoFriendsResponderModel.AudioSettings.Messages, _autoFriendsResponderModel.WelcomeCount, userIds, _autoFriendsResponderModel.Delay);
            }

            if (_autoFriendsResponderModel.SetLikeToProfile)
            {
                for (int i = 0; i < likeProfileAndLikeWallCountAndAddToFriendCount; i++)
                {
                    Thread.Sleep(_autoFriendsResponderModel.Delay * 1000);
                    await SetLikeToProfile(api, requestFriends.Items.ElementAt(i));
                }
            }

            if (_autoFriendsResponderModel.SetLikeToWall)
            {
                for (int i = 0; i < likeProfileAndLikeWallCountAndAddToFriendCount; i++)
                {
                    Thread.Sleep(_autoFriendsResponderModel.Delay * 1000);
                    await SetLikeToWall(api, requestFriends.Items.ElementAt(i));
                }
            }
        }

        private async Task SpecificUsersWorker(VkNet.VkApi api, AutoFriendsResponderModel _autoFriendsResponderModel)
        {
            var userIds = await ResolveGroupOrUserNames(api, _autoFriendsResponderModel.UserNamesOrIds, VkObjectType.User);

            var specificUsers =
                (await api.Users.GetAsync(userIds, ProfileFields.All)).Where(x => x?.IsClosed != true && x?.IsDeactivated != true);

            var specificUsersIdsWhereAccessWriteMessage = specificUsers.Where(x => x.CanWritePrivateMessage).Select(x => x.Id).ToList();

            if (_autoFriendsResponderModel.MessageSettings != null &&
                _autoFriendsResponderModel.MessageSettings.TextMessages != null &&
                _autoFriendsResponderModel.MessageSettings.TextMessages.Any())
            {
                var userIdsForMessage = await FilterByConversation(api, _autoFriendsResponderModel.MessageSettings.ConversationTypeEvent,
                    specificUsersIdsWhereAccessWriteMessage);
                await SendMessages(api, _autoFriendsResponderModel.MessageSettings.TextMessages,
                    _autoFriendsResponderModel.WelcomeCount, userIdsForMessage, _autoFriendsResponderModel.Delay);
            }

            if (_autoFriendsResponderModel.PhotoOrVideoSettings != null &&
                _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath != null &&
                _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath.Any())
            {
                await SendMessageWithPhoto(api, _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath,
                    _autoFriendsResponderModel.PhotoOrVideoSettings.Messages, _autoFriendsResponderModel.WelcomeCount, specificUsersIdsWhereAccessWriteMessage, _autoFriendsResponderModel.Delay);
            }

            if (_autoFriendsResponderModel.AudioSettings != null &&
                _autoFriendsResponderModel.AudioSettings.AudioFilesPath != null &&
                _autoFriendsResponderModel.AudioSettings.AudioFilesPath.Any())
            {
                await SendMessageWithAudio(api, _autoFriendsResponderModel.AudioSettings.AudioFilesPath,
                    _autoFriendsResponderModel.AudioSettings.Messages, _autoFriendsResponderModel.WelcomeCount, specificUsersIdsWhereAccessWriteMessage, _autoFriendsResponderModel.Delay);
            }

            if (_autoFriendsResponderModel.SetLikeToProfile)
            {
                foreach (var user in specificUsers)
                {
                    Thread.Sleep(_autoFriendsResponderModel.Delay * 1000);
                    await SetLikeToProfile(api, user.Id);
                }
            }

            if (_autoFriendsResponderModel.SetLikeToWall)
            {
                foreach (var user in specificUsers)
                {
                    Thread.Sleep(_autoFriendsResponderModel.Delay * 1000);
                    await SetLikeToWall(api, user.Id);
                }
            }

            if (_autoFriendsResponderModel.AddToFriends)
            {
                foreach (var user in specificUsers)
                {
                    Thread.Sleep(_autoFriendsResponderModel.Delay * 1000);
                    await api.Friends.AddAsync(user.Id, "", false);
                }
            }
        }

        private async Task SpecificGroupsWorker(VkNet.VkApi api, AutoFriendsResponderModel _autoFriendsResponderModel)
        {
            var groups = (await api.Groups.GetByIdAsync(_autoFriendsResponderModel.GroupNamesOrIds, null, GroupsFields.All)).Where(x => x.IsClosed == GroupPublicity.Public);

            if (groups != null && groups.Any())
            {
                var groupsIds = groups.Select(x => x.Id).ToList();
                var groupsIdsWhereAccessWriteMessage = groups.Select(x => x.Id).ToList();

                if (_autoFriendsResponderModel.MessageSettings != null &&
                    _autoFriendsResponderModel.MessageSettings.TextMessages != null &&
                    _autoFriendsResponderModel.MessageSettings.TextMessages.Any())
                {
                    var groupsIdsForMessage = await FilterByConversation(api,
                        _autoFriendsResponderModel.MessageSettings.ConversationTypeEvent, groupsIdsWhereAccessWriteMessage,
                        IsGroup: true);

                    await SendMessages(api, _autoFriendsResponderModel.MessageSettings.TextMessages,
                        _autoFriendsResponderModel.WelcomeCount, groupsIdsForMessage, _autoFriendsResponderModel.Delay, IsGroup: true);
                }

                if (_autoFriendsResponderModel.PhotoOrVideoSettings != null &&
                    _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath != null &&
                    _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath.Any())
                {
                    await SendMessageWithPhoto(api, _autoFriendsResponderModel.PhotoOrVideoSettings.PhotoFilesPath,
                        _autoFriendsResponderModel.PhotoOrVideoSettings.Messages, _autoFriendsResponderModel.WelcomeCount, groupsIdsWhereAccessWriteMessage, _autoFriendsResponderModel.Delay);
                }


                if (_autoFriendsResponderModel.AudioSettings != null &&
                    _autoFriendsResponderModel.AudioSettings.AudioFilesPath != null &&
                    _autoFriendsResponderModel.AudioSettings.AudioFilesPath.Any())
                {
                    await SendMessageWithAudio(api, _autoFriendsResponderModel.AudioSettings.AudioFilesPath,
                        _autoFriendsResponderModel.AudioSettings.Messages, _autoFriendsResponderModel.WelcomeCount, groupsIdsWhereAccessWriteMessage, _autoFriendsResponderModel.Delay);
                }

                if (_autoFriendsResponderModel.SetLikeToWall)
                {
                    foreach (var groupId in groupsIds)
                    {
                        Thread.Sleep(_autoFriendsResponderModel.Delay * 1000);
                        await SetLikeToWall(api, groupId, IsGroup: true);
                    }
                }
            }
        }

        private async Task<List<long>> FilterByConversation(VkNet.VkApi api, ConversationType conversationType,
            List<long> Ids, bool IsGroup = false)
        {
            var updateUserIdList = new List<long>();

            switch (conversationType)
            {
                case ConversationType.ConversationIsEmpty:
                    {
                        foreach (var id in Ids)
                        {
                            var history = IsGroup
                                ? await api.Messages.GetHistoryAsync(new MessagesGetHistoryParams()
                                {
                                    PeerId = -id,
                                })
                                : await api.Messages.GetHistoryAsync(new MessagesGetHistoryParams()
                                {
                                    UserId = id,
                                });

                            if (history?.Messages is null || !history.Messages.Any())
                                updateUserIdList.Add(id);
                        }

                        return updateUserIdList;
                    }
                case ConversationType.ConversationIsEmptyOrNoAnwerFromMe:
                    {
                        foreach (var id in Ids)
                        {
                            var history = IsGroup
                                ? await api.Messages.GetHistoryAsync(new MessagesGetHistoryParams()
                                {
                                    PeerId = -id,
                                })
                                : await api.Messages.GetHistoryAsync(new MessagesGetHistoryParams()
                                {
                                    UserId = id,
                                });

                            if (history?.Messages is null || !history.Messages.Any())
                            {
                                updateUserIdList.Add(id);
                            }
                            else if (history.Messages.Any())
                            {
                                var outgoingMessages = history.Messages.Where(x => x.Out == false);

                                if (outgoingMessages.Any())
                                    updateUserIdList.Add(id);
                            }
                        }

                        return updateUserIdList;
                    }
                case ConversationType.AnyCase:
                    return Ids;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conversationType), conversationType, null);
            }
        }

        private async Task SetLikeToProfile(VkNet.VkApi api, long userId)
        {
            try
            {
                var photos = await api.Photo.GetAsync(new PhotoGetParams
                {
                    OwnerId = userId,
                    AlbumId = PhotoAlbumType.Profile,
                });

                if (photos != null && photos.Any())
                {
                    var photoId = (long)photos.Last().Id;
                    await api.Likes.AddAsync(new LikesAddParams
                    {
                        Type = LikeObjectType.Photo,
                        OwnerId = userId,
                        ItemId = photoId
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    "Set Like to profile exception. Profile Info: FirstName: {FirstName}, LastName: {LastName}. Exception: {exception}",
                    api.Account.GetProfileInfo().FirstName, api.Account.GetProfileInfo().LastName, e.Message);

                throw e;
            }
        }

        private async Task SetLikeToWall(VkNet.VkApi api, long id, bool IsGroup = false)
        {
            try
            {
                id = IsGroup ? -id : id;
                var post = await api.Wall.GetAsync(new WallGetParams
                {
                    OwnerId = id,
                    Count = 1
                });

                if (post != null && post.WallPosts.Any())
                {
                    var postId = (long)post.WallPosts.First().Id;
                    await api.Likes.AddAsync(new LikesAddParams
                    {
                        Type = LikeObjectType.Post,
                        OwnerId = id,
                        ItemId = postId
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    "Set Like to wall exception. Profile Info: FirstName: {FirstName}, LastName: {LastName}. Exception: {exception}",
                    api.Account.GetProfileInfo().FirstName, api.Account.GetProfileInfo().LastName, e.Message);

                throw e;
            }
        }

        private async Task SendMessages(VkNet.VkApi api, List<string> welcomeMessages, int welcomeCount, List<long> ids,
            int delay, bool IsGroup = false)
        {
            welcomeCount = ids.Count < welcomeCount ? ids.Count : welcomeCount;

            for (int i = 0; i < welcomeCount; i++)
            {
                Thread.Sleep(delay * 1000);

                try
                {
                    var result = IsGroup
                        ? await api.Messages.SendAsync(new MessagesSendParams()
                        {
                            PeerId = -ids.ElementAt(i),
                            Message = welcomeMessages.Any() ? welcomeMessages.ElementAt(new Random().Next(0, welcomeMessages.Count)) : null,
                            RandomId = new Random().Next()
                        })
                        : await api.Messages.SendAsync(new MessagesSendParams()
                        {
                            UserId = ids.ElementAt(i),
                            Message = welcomeMessages.Any() ? welcomeMessages.ElementAt(new Random().Next(0, welcomeMessages.Count)) : null,
                            RandomId = new Random().Next()
                        });

                }
                catch (Exception e)
                {
                    _logger.LogError(
                        "Send Messages exception. Profile Info: FirstName: {FirstName}, LastName: {LastName}. Exception: {exception}",
                        api.Account.GetProfileInfo().FirstName, api.Account.GetProfileInfo().LastName, e.Message);
                }
            }
        }

        private async Task SendMessageWithPhoto(VkNet.VkApi api, List<string> photoFilesPath, List<string> welcomeMessages, int welcomeCount,
            List<long> ids, int delay, bool IsGroup = false)
        {
            try
            {
                welcomeCount = ids.Count < welcomeCount ? ids.Count : welcomeCount;

                for (int i = 0; i < welcomeCount; i++)
                {
                    Thread.Sleep(delay * 1000);

                    if (!File.Exists(photoFilesPath.ElementAt(i)))
                        throw new Exception($"File '{photoFilesPath.ElementAt(i)}' Is not Found");

                    var albumForLoad = api.Photo.CreateAlbum(new PhotoCreateAlbumParams
                    {
                        Title = "AlbumForLoad"
                    });

                    // Получить адрес сервера для загрузки.
                    var uploadServer = api.Photo.GetUploadServer(albumForLoad.Id);

                    // Загрузить файл.
                    var wc = new WebClient();
                    var responseFile =
                        Encoding.ASCII.GetString(wc.UploadFile(uploadServer.UploadUrl, photoFilesPath.ElementAt(i)));

                    // Сохранить загруженный файл
                    var photo = await api.Photo.SaveAsync(new PhotoSaveParams
                    {
                        SaveFileResponse = responseFile,
                        AlbumId = albumForLoad.Id
                    });

                    var result = IsGroup
                        ? await api.Messages.SendAsync(new MessagesSendParams()
                        {
                            PeerId = -ids.ElementAt(i),
                            Attachments = photo,
                            Message = welcomeMessages.Any() ? welcomeMessages.ElementAt(new Random().Next(0, welcomeMessages.Count)) : null,
                            RandomId = new Random().Next()
                        })
                        : await api.Messages.SendAsync(new MessagesSendParams()
                        {
                            UserId = ids.ElementAt(i),
                            Attachments = photo,
                            Message = welcomeMessages.Any() ? welcomeMessages.ElementAt(new Random().Next(0, welcomeMessages.Count)) : null,
                            RandomId = new Random().Next()
                        });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    "Send Messages with photo exception. Profile Info: FirstName: {FirstName}, LastName: {LastName}. Exception: {exception}",
                    api.Account.GetProfileInfo().FirstName, api.Account.GetProfileInfo().LastName, e.Message);

                throw e;
            }
            finally
            {
                foreach (var photoFilepath in photoFilesPath.Where(File.Exists))
                {
                    File.Delete(photoFilepath);
                }
            }
        }

        private async Task SendMessageWithAudio(VkNet.VkApi api, List<string> audioFilesPath, List<string> welcomeMessages, int welcomeCount,
            List<long> ids, int delay, bool IsGroup = false)
        {
            try
            {
                welcomeCount = ids.Count < welcomeCount ? ids.Count : welcomeCount;

                for (int i = 0; i < welcomeCount; i++)
                {
                    Thread.Sleep(delay * 1000);

                    if (!File.Exists(audioFilesPath.ElementAt(i)))
                        throw new Exception($"File '{audioFilesPath.ElementAt(i)}' Is not Found");

                    // Получить адрес сервера для загрузки.
                    var uploadServer = api.Audio.GetUploadServer();

                    // Загрузить файл.
                    var wc = new WebClient();
                    var responseFile =
                        Encoding.ASCII.GetString(wc.UploadFile(uploadServer, audioFilesPath.ElementAt(i)));

                    // Сохранить загруженный файл
                    var audio = await api.Audio.SaveAsync(responseFile);

                    var result = IsGroup
                        ? await api.Messages.SendAsync(new MessagesSendParams()
                        {
                            PeerId = -ids.ElementAt(i),
                            Attachments = new List<MediaAttachment>
                            {
                                audio
                            },
                            Message = welcomeMessages.Any() ? welcomeMessages.ElementAt(new Random().Next(0, welcomeMessages.Count)) : null,
                            RandomId = new Random().Next()
                        })
                        : await api.Messages.SendAsync(new MessagesSendParams()
                        {
                            UserId = ids.ElementAt(i),
                            Attachments = new List<MediaAttachment>
                            {
                                audio
                            },
                            Message = welcomeMessages.ElementAt(new Random().Next(0, welcomeMessages.Count)),
                            RandomId = new Random().Next()
                        });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    "Send Messages with audio exception. Profile Info: FirstName: {FirstName}, LastName: {LastName}. Exception: {exception}",
                    api.Account.GetProfileInfo().FirstName, api.Account.GetProfileInfo().LastName, e.Message);
                throw e;
            }
            finally
            {
                foreach (var audioFilepath in audioFilesPath.Where(File.Exists))
                {
                    File.Delete(audioFilepath);
                }
            }
        }

        private async Task<List<long>> ResolveGroupOrUserNames(VkNet.VkApi api, List<string> names, VkObjectType type)
        {
            var ids = new List<long>();

            foreach (var name in names.Where(name => !string.IsNullOrEmpty(name)))
            {
                if (long.TryParse(name, out var id))
                {
                    ids.Add(id);
                }
                else
                {
                    var item = await api.Utils.ResolveScreenNameAsync(name);
                    if (item.Type == type && item.Id.HasValue)
                        ids.Add(item.Id.Value);
                }
            }

            return ids;
        }

        public async Task<string> GetFileInfo(string path)
        {
            var homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                               Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            return homePath;

            //var pathToFile = Path.Combine(homePath, "inbox/ftp/test.jpg");
            
            //if (!File.Exists(pathToFile))
            //    return "File not found " + pathToFile;

            //return path;
        }
    }
}
