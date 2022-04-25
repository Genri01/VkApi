﻿using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkApi.Services;
using VkApi.SettingsEvent.AutoAddedFriends;
using VkApi.SettingsEvent.AutoLikingFriends;
using VkApi.SettingsEvent.AutoResponder;
using VkApi.SettingsEvent.SuggestFriendsFilter;

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

        [HttpPost("addSuggestionsFriends")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddFriends([FromHeader] string token, [FromBody] AddSuggestFriendsModel _addSuggestFriendsModel, CancellationToken cancellationToken = default)
        {
            await _vkService.AddSuggestFriends(token, _addSuggestFriendsModel);
            return new OkResult();
        }

        [HttpPost("autoResponderFriends")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> AutoResponderFriends([FromHeader] string token, [FromBody] AutoFriendsResponderModel _autoFriendsResponderModel, CancellationToken cancellationToken = default)
        {
            await _vkService.AutoResponderFriends(token, _autoFriendsResponderModel);
            return new OkResult();
        }

        [HttpPost("filterSuggestionsFriends")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> FilterSuggestionsFriends([FromHeader] string token, [FromBody] SuggestFriendsFilter friendsFilter, CancellationToken cancellationToken = default)
        {
            var suggestionsFriends = await _vkService.FilterSuggestionsFriends(token, friendsFilter);
            return new OkObjectResult(suggestionsFriends);
        }

        [HttpPost("autoLikingFriendsOrGroups")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> AutoLikingFriendsOrGroups([FromHeader] string token, [FromBody] AutoLikingFriends autoLikingFriends, CancellationToken cancellationToken = default)
        {
            await _vkService.AutoLikingFriendsOrGroups(token, autoLikingFriends);
            return new OkResult();
        }

        /// <summary>
        /// Retrieves a specific product by unique id
        /// </summary>
        /// <remarks>Awesomeness!</remarks>
        /// <response code="200">Product created</response>
        /// <response code="400">Product has missing/invalid values</response>
        /// <response code="500">Oops! Can't create your product right now</response>
        [HttpPost("getMembersFromGroup")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetGroups([FromHeader] string token, [FromBody] string groupName, CancellationToken cancellationToken = default)
        {
            var groups = await _vkService.GetMembersFromGroup(token, groupName);
            return new OkObjectResult(groups);
        }


        [HttpGet("GetPath")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPath(CancellationToken cancellationToken = default)
        {

            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                               Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            string currentDirectory = Directory.GetCurrentDirectory();

            if (!Directory.Exists(Path.Combine(homePath, "ShareFolderForVk")))
                Directory.CreateDirectory(Path.Combine(currentDirectory, "ShareFolderForVk"));

            string str = "";

            str = str + "homePath: " + homePath + " ";
            str = str + "currentDirectory: " + currentDirectory;

            return new OkObjectResult(str);
        }
    }
}
