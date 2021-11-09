using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace WPF_Telegram_Bot
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ObservableCollection<string> UserMessages { get; set; }

        public User()
        { }
       public User(long id, string name, ObservableCollection<string> userMessages)
        {
            Id = id;
            Name = name;
            UserMessages = userMessages;
        }


    }
}
