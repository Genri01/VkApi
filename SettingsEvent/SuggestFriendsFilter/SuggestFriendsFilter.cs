namespace VkApi.SettingsEvent.SuggestFriendsFilter
{
    public class SuggestFriendsFilter
    {
        public int Count { get; set; }
        public SuggestFriendsFilterType SuggestFriendsFilterType { get; set; } = SuggestFriendsFilterType.Mutual;
    }

    public enum SuggestFriendsFilterType
    {
        /// <summary>Пользователи, с которыми много общих друзей;</summary>
        Mutual,

        /// <summary>
        /// Пользователи, найденные с помощью метода account.importContacts;
        /// </summary>
        Contacts,

        /// <summary>
        /// Пользователи, которые импортировали те же контакты, что и текущий пользователь,
        /// используя метод
        /// account.importContacts;
        /// </summary>
        MutualContacts
    }
}
