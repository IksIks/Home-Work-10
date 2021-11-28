using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;


namespace WPF_Telegram_Bot
{
    public class SerializerLog
    {
        /// <summary>
        /// сериализация выполняемая при первом создании Log пользователя
        /// </summary>
        /// <param name="userLogs">коллекция всех сообщений, отправленных боту, от всех пользователей</param>
        /// <param name="Id">Id пользователя</param>
        /// <param name="Name">Имя пользователя</param>
        /// <returns></returns>
        public string Serialize (ObservableCollection<UserLog> userLogs, long Id, string Name)
        {
            JObject joUser = new JObject();
            JArray jaMessages = new JArray();
            joUser["Время записи"] = DateTime.Now.ToShortTimeString();
            joUser["Id"] = Id;
            joUser["Имя Пользователя"] = Name;
            
            foreach (var item in userLogs)
            {
                JObject joMessage = new JObject();
                if (item.Id == Id)
                {
                    joMessage["Время отправки"] = (item.MessageTime.ToLocalTime()).ToShortTimeString();
                    joMessage["Текст"] = item.MessageText;
                    jaMessages.Add(joMessage);
                }
            }
            joUser["Сообщения"] = jaMessages;
            return joUser.ToString();
        }

        /// <summary>
        /// сериализация выполняемая при дозаписывании Log пользователя
        /// </summary>
        /// <param name="userLogs">коллекция всех сообщений, отправленных боту, от всех пользователей</param>
        /// <param name="Id">Id пользователя</param>
        /// <param name="lastChanges">значение последнего изменения(записи) файла .json</param>
        /// <returns></returns>
        public string Serialize(ObservableCollection<UserLog> userLogs, long Id, DateTime lastChanges)
        {
            JObject joUser = new JObject();
            JArray jaMessages = new JArray();
            joUser["Время записи"] = DateTime.Now.ToShortTimeString();
            foreach (var item in userLogs)
            {
                JObject joMessage = new JObject();
                if (item.Id == Id & item.MessageTime.ToLocalTime() > lastChanges)
                {
                    joMessage["Время отправки"] = (item.MessageTime.ToLocalTime()).ToShortTimeString();
                    joMessage["Текст"] = item.MessageText;
                    jaMessages.Add(joMessage);
                }
            }
            joUser["Сообщения"] = jaMessages;
            return joUser.ToString();
        }
    }
}
