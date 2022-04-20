using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkApi.SettingsEvent.AutoSecretary
{
    public class AutoSecretaryResponder
    {
        public string AcountToken { get; set; } // токены пользователей
        public int CountAnswer { get; set; } //количество автоответов
        public List<Rules> Rules { get; set; } //количество автоответов
    }
}
