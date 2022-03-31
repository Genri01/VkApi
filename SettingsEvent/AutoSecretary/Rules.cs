using System.Collections.Generic;
using VkApi.SettingsEvent.AutoResponder;

namespace VkApi.SettingsEvent.AutoSecretary
{
    public class Rules
    {
        public List<string> KeyWords { get; set; }
        public string Message { get; set; }
        public PhotoOrVideoSettings PhotoOrVideoSettings { get; set; } // настройка отправки фото или видео
        public AudioSettings AudioSettings { get; set; } // настройка отправки аудио из файла
    }
}
