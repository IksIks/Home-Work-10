using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace WPF_Telegram_Bot
{
    public class UserLog
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string MessageText { get; set; }


        public UserLog()
        { }
       public UserLog(long id, string name, string messageText)
        {
            Id = id;
            Name = name;
            MessageText = messageText;
        }

        public override string ToString()
        {
            return $"Id {Id}, Пользователь {Name}, Текст {MessageText}";
        }
    }
}
