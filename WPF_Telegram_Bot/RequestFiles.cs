using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;


namespace WPF_Telegram_Bot
{
    class RequestFiles
    {
        //список имен файлов
        public static List<FileInfo> FileListName { get; private set; }
        public static async void ProcessRequestFiles(string fileType, string firstName)
        {
            DirectoryInfo files = new DirectoryInfo(MainWindow.Path + $@"\{firstName}\{fileType}\");
            
            FileListName = files.GetFiles().ToList();
            if (FileListName.Count == 0)
            {
                await MainWindow.bot.SendTextMessageAsync(MainWindow.Message.Chat.Id, "Нет сохраненных файлов, отправьте мне что-нибудь для начала");
                return;
            }
            List<List<InlineKeyboardButton>> fileButtons = new List<List<InlineKeyboardButton>>();

            //цикл создания Inline клавитатуры найденных файлов
            for (int i = 0; i < FileListName.Count; i++)
            {
                List<InlineKeyboardButton> buttonArray = new List<InlineKeyboardButton>();
                InlineKeyboardButton button = new InlineKeyboardButton();
                button.CallbackData = fileType + "." + i.ToString();
                button.Text = FileListName[i].ToString();
                buttonArray.Add(button);
                fileButtons.Add(buttonArray);
            }
            InlineKeyboardMarkup InlineKeyboardMarkup = new InlineKeyboardMarkup(fileButtons);
            await MainWindow.bot.SendTextMessageAsync(MainWindow.Message.Chat.Id, $"Список файлов", replyMarkup: InlineKeyboardMarkup);

        }
    }
}