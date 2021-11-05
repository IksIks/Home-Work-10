using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Cloud.Dialogflow.V2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace WPF_Telegram_Bot
{
    


    public partial class MainWindow : Window
    {

        public static TelegramBotClient bot;
        private static string firstName = default;
        private static SessionsClient dFlowClient;
        private static string projectID;
        private static string sessionID;
        private static string path;
        public static Telegram.Bot.Types.Message Messages { get; set; }
        //Начальный путь к файлу
        public static string Path
        {
            get { return @"c:\Telegram_Bot_Users"; }
            private set { path = value; }
        }

        /// <summary>
        /// Скачивание файлов
        /// </summary>
        /// <param name="fileId">ID файла отправленное с сервера Telegram</param>
        /// <param name="path">путь куда сохраняется файл</param>
        private static async void Download(string fileId, string path)
        {
            var file = await bot.GetFileAsync(fileId);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await bot.DownloadFileAsync(file.FilePath, stream);
            }
        }

        /// <summary>
        /// Отправка файла обратно пользователю
        /// </summary>
        /// <param name="path">Путь где лежит файл</param>
        /// <param name="fileName">имя файла</param>
        /// <param name="messageType">Тип документа (Document, Video, etc..)</param>
        private static async void SendFiles(string path, string fileName, string messageType)
        {

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                switch (messageType)
                {
                    case ("Document"):
                        {
                            Console.WriteLine($"Отправлен файл {fileName}");
                            await bot.SendTextMessageAsync(Messages.Chat.Id, "Уже загружаю....");
                            await bot.SendDocumentAsync(Messages.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs, fileName));
                            break;
                        }
                    case "Video":
                        {
                            Console.WriteLine($"Отправлен файл {fileName}");
                            await bot.SendTextMessageAsync(Messages.Chat.Id, "Уже загружаю....");
                            await bot.SendVideoAsync(Messages.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs, fileName));
                            break;
                        }
                    case "Audio":
                        {
                            Console.WriteLine($"Отправлен файл {fileName}");
                            await bot.SendTextMessageAsync(Messages.Chat.Id, "Уже загружаю....");
                            await bot.SendAudioAsync(Messages.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs, fileName));
                            break;
                        }
                    case "Photo":
                        {
                            Console.WriteLine($"Отправлен файл {fileName}");
                            await bot.SendTextMessageAsync(Messages.Chat.Id, "Уже загружаю....");
                            await bot.SendPhotoAsync(Messages.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs, fileName));
                            break;
                        }
                }
            }

        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnClick(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Name)
            {
                case "Document":
                    {
                        
                        break;
                    }
                case "Audio":
                    {
                        break;
                    }
                case "Photo":
                    {
                        break;
                    }
                case "Video":
                    {
                        break;
                    }
            }

        }
    }
}
