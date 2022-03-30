using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkApi.SettingsEvent
{
    public class EventSettings
    {
        public int id { get; set; }
        public SettingsAcc settingsAcc { get; set; }
        public Tasks tasks { get; set; }
    }

    public class Vpn
    {
        public string country { get; set; }
    }

    public class Proxy
    {
        public string ip { get; set; }
        public string log { get; set; }
        public string pass { get; set; }
    }

    public class Network
    {
        public bool statusON { get; set; }
        public Vpn vpn { get; set; }
        public Proxy proxy { get; set; }
    }

    public class SettingsAcc
    {
        public string name { get; set; }
        public bool visibleSite { get; set; }
        public bool visibleImg { get; set; }
        public string typeAcc { get; set; }
        public string anticapcha { get; set; }
        public Network network { get; set; }
    }

    public class Days
    {
        public List<string> day { get; set; }
        public bool all { get; set; }
    }

    public class Auto
    {
        public Days days { get; set; }
        public string startShedule { get; set; }
        public string stopShedule { get; set; }
    }

    public class Manual
    {
        public string stopShedule { get; set; }
    }

    public class Mode
    {
        public Auto auto { get; set; }
        public Manual manual { get; set; }
        public List<string> task { get; set; }
        public bool errinTG { get; set; }
    }

    public class Shedule
    {
        public Mode mode { get; set; }
    }

    public class MessageSettings
    {
        public bool about { get; set; }
        public bool noanswer { get; set; }
        public bool anyСase { get; set; }
    }

    public class RandomizeText
    {
        public bool on { get; set; }
        public List<string> text { get; set; }
        public bool random { get; set; }
        public bool username { get; set; }
    }

    public class LinkPhoto
    {
        public bool on { get; set; }
        public List<string> link { get; set; }
        public bool random { get; set; }
    }

    public class Audio
    {
        public bool on { get; set; }
        public List<string> path { get; set; }
        public bool random { get; set; }
    }

    public class AutoresponderСonfirmFriends
    {
        public int contGreeting { get; set; }
        public int delay { get; set; }
        public MessageSettings messageSettings { get; set; }
        public RandomizeText randomizeText { get; set; }
        public LinkPhoto linkPhoto { get; set; }
        public Audio audio { get; set; }
    }

    public class OneOption
    {
        public string text { get; set; }
        public RandomizeText randomizeText { get; set; }
        public LinkPhoto linkPhoto { get; set; }
        public Audio audio { get; set; }
    }

    public class TwoOption
    {
        public string text { get; set; }
        public RandomizeText randomizeText { get; set; }
        public LinkPhoto linkPhoto { get; set; }
        public Audio audio { get; set; }
    }

    public class ThreeOption
    {
        public string text { get; set; }
        public RandomizeText randomizeText { get; set; }
        public LinkPhoto linkPhoto { get; set; }
        public Audio audio { get; set; }
    }

    public class Autosecretary
    {
        public int countAnswer { get; set; }
        public int delay { get; set; }
        public OneOption oneOption { get; set; }
        public TwoOption twoOption { get; set; }
        public ThreeOption threeOption { get; set; }
    }

    public class LikingViewingStories
    {
        public int countUser { get; set; }
        public int delay { get; set; }
        public bool randdomLike { get; set; }
    }

    public class AutoresponderIncomingRequestsFriends
    {
        public int contGreeting { get; set; }
        public int delay { get; set; }
        public bool likeAva { get; set; }
        public bool likeWall { get; set; }
        public MessageSettings messageSettings { get; set; }
        public RandomizeText randomizeText { get; set; }
        public LinkPhoto linkPhoto { get; set; }
        public Audio audio { get; set; }
    }

    public class SendingMessagesUserList
    {
        public int countMessage { get; set; }
        public int delay { get; set; }
        public bool likeAva { get; set; }
        public bool likeWall { get; set; }
        public MessageSettings messageSettings { get; set; }
        public RandomizeText randomizeText { get; set; }
        public LinkPhoto linkPhoto { get; set; }
        public Audio audio { get; set; }
        public List<string> linkList { get; set; }
    }

    public class PossibleFriends
    {
        public int countFrendsReq { get; set; }
        public int inRequestMax { get; set; }
        public int delay { get; set; }
        public bool likeAva { get; set; }
        public bool likeWall { get; set; }
    }

    public class TargetAudienceFromList
    {
        public int countFrendsReq { get; set; }
        public int inRequestMax { get; set; }
        public int delay { get; set; }
        public bool addFriends { get; set; }
        public bool lookStory { get; set; }
        public bool likeWall { get; set; }
        public bool likeAva { get; set; }
        public List<string> listLink { get; set; }
    }

    public class PublishingStories
    {
        public int countPublish { get; set; }
        public int delay { get; set; }
        public string linkButtonMore { get; set; }
        public string materialHistory { get; set; }
    }

    public class ParserTargetAudience
    {
        public List<string> phrases { get; set; }
        public string country { get; set; }
        public string cityes { get; set; }
        public List<string> linkUserList { get; set; }
    }

    public class SendMessagesCommunityList
    {
        public int countСommunities { get; set; }
        public int delay { get; set; }
        public MessageSettings messageSettings { get; set; }
        public RandomizeText randomizeText { get; set; }
        public LinkPhoto linkPhoto { get; set; }
        public Audio audio { get; set; }
        public List<string> linkListСommunities { get; set; }
    }

    public class Tasks
    {
        public Shedule shedule { get; set; }
        public AutoresponderСonfirmFriends autoresponderСonfirmFriends { get; set; }
        public Autosecretary autosecretary { get; set; }
        public LikingViewingStories likingViewingStories { get; set; }
        public AutoresponderIncomingRequestsFriends autoresponderIncomingRequestsFriends { get; set; }
        public SendingMessagesUserList sendingMessagesUserList { get; set; }
        public PossibleFriends possibleFriends { get; set; }
        public TargetAudienceFromList targetAudienceFromList { get; set; }
        public PublishingStories publishingStories { get; set; }
        public ParserTargetAudience parserTargetAudience { get; set; }
        public SendMessagesCommunityList sendMessagesCommunityList { get; set; }
    }
}
