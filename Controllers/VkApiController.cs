﻿using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VkApi.Services;
using VkApi.SettingsEvent.AutoAddedFriends;
using VkApi.SettingsEvent.AutoResponder;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace VkApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VkApiController : ControllerBase
    {
        private readonly IVkService _vkService;

        public VkApiController(IVkService vkService)
        {
            _vkService = vkService;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetVkSettings([FromHeader] string token, CancellationToken cancellationToken = default)
        {
            var settings = await _vkService.GetVkSettings(token);
            return new OkObjectResult(settings);
        }

        [HttpPost("sendMessages")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendMessages([FromHeader] string token, [FromBody] MessagesSendParams messageParams, CancellationToken cancellationToken = default)
        {
            await _vkService.SendMessage(token, messageParams);
            return new OkResult();
        }

        [HttpPost("addSuggestionsFriends")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddFriends([FromBody] AddSuggestFriendsModel _addSuggestFriendsModel, CancellationToken cancellationToken = default)
        {
            await _vkService.AddSuggestFriends(_addSuggestFriendsModel);
            return new OkResult();
        }

        [HttpPost("autoResponderFriends")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> AutoResponderFriends([FromBody] AutoFriendsResponderModel _autoFriendsResponderModel, CancellationToken cancellationToken = default)
        {
            await _vkService.AutoResponderFriends(_autoFriendsResponderModel);
            return new OkResult();
        }

        [HttpPost("filterSuggestionsFriends")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> FilterSuggestionsFriends([FromHeader] string token, [FromBody] FriendsFilter friendsFilter, CancellationToken cancellationToken = default)
        {
            var suggestionsFriends = await _vkService.FilterSuggestionsFriends(token, friendsFilter);
            return new OkObjectResult(suggestionsFriends);
        }

        //[HttpPost("getMembersFromGroup")]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //public async Task<IActionResult> GetGroups([FromHeader] string token, [FromBody] GroupsGetMembersParams groupsGetMembersParams, string groupName, CancellationToken cancellationToken = default)
        //{
        //    var groups = await _vkService.Get(token, groupsGetMembersParams, groupName);
        //    return new OkObjectResult(groups);
        //}
    }
}
