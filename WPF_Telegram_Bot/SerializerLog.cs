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
        public string Serialize (ObservableCollection<UserLog> userLogs, long Id, string Name)
        {
            JObject json = new JObject();
            JObject joUser = new JObject();
            JArray jaMessages = new JArray();
            joUser["Id"] = Id;
            joUser["Имя Пользователя"] = Name;
            joUser["Дата сообщениий"] = userLogs[0].MessageTime.ToShortDateString();
            
            foreach (var item in userLogs)
            {
                if (item.Id == Id)
                {
                    jaMessages.Add(item.MessageText);
                }
            }
            joUser["Сообщения"] = jaMessages;
            return joUser.ToString();
        }
    }
}
