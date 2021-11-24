using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
using Google.Cloud.Dialogflow.V2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
using System.IO;
//using System.Linq;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace WPF_Telegram_Bot
{
    public partial class MainWindow : Window
    {
        #region Поля и свойства класса
        public static TelegramBotClient bot;
        private static string firstName = default;
        private static SessionsClient dFlowClient;
        private static string projectID;
        private static string sessionID;
        private static string path;

        // свойство для передачи в ListBox "UserList"
        public static ObservableCollection<UserLog> ListBoxUserList { get; set; }

        //переменная для возможности передачи в основной поток
        private static MainWindow window;

        // свойство для записи вводимого администратором текста для отправки его выбранному пользователю
        public string TextToUser { get; set; }

        // свойство для записи логов всех подключаемых пользователей и передачи в ListBox "CMD"
        public static ObservableCollection<UserLog> ListBoxUsersBotLogsCmd { get; set; }
        public static Telegram.Bot.Types.Message Message { get; set; }
        #endregion


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
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    await bot.DownloadFileAsync(file.FilePath, stream);
                }
            }
            catch (Exception)
            {

                return;
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
                            await bot.SendTextMessageAsync(Message.Chat.Id, "Уже загружаю....");
                            await bot.SendDocumentAsync(Message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs, fileName));
                            break;
                        }
                    case "Video":
                        {
                            Console.WriteLine($"Отправлен файл {fileName}");
                            await bot.SendTextMessageAsync(Message.Chat.Id, "Уже загружаю....");
                            await bot.SendVideoAsync(Message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs, fileName));
                            break;
                        }
                    case "Audio":
                        {
                            Console.WriteLine($"Отправлен файл {fileName}");
                            await bot.SendTextMessageAsync(Message.Chat.Id, "Уже загружаю....");
                            await bot.SendAudioAsync(Message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs, fileName));
                            break;
                        }
                    case "Photo":
                        {
                            Console.WriteLine($"Отправлен файл {fileName}");
                            await bot.SendTextMessageAsync(Message.Chat.Id, "Уже загружаю....");
                            await bot.SendPhotoAsync(Message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs, fileName));
                            break;
                        }
                }
            }

        }

        [Obsolete]
        public MainWindow()
        {
            InitializeComponent();
            

            window = this;
            //string tokenBot = System.IO.File.ReadAllText(@"C:\Dropbox\IKS\C# проекты\C# Учеба\ДЗ 10\Token_BOT.txt");
            //string dFlowKeyPath = @"C:\Dropbox\IKS\C# проекты\C# Учеба\ДЗ 10\WPF_Telegram_Bot\iksbot-9tan-8bfc6cdbd2be.json";
            string tokenBot = System.IO.File.ReadAllText(@"D:\Dropbox\IKS\C# проекты\C# Учеба\ДЗ 10\Token_BOT.txt");
            string dFlowKeyPath = @"D:\Dropbox\IKS\C# проекты\C# Учеба\ДЗ 10\WPF_Telegram_Bot\iksbot-9tan-8bfc6cdbd2be.json";

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            #region BotAnswerInitiation
            bot = new TelegramBotClient(tokenBot);
            var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(dFlowKeyPath));
            projectID = dic["project_id"];
            sessionID = dic["private_key_id"];
            var dialogFlowBilder = new SessionsClientBuilder { CredentialsPath = dFlowKeyPath };
            dFlowClient = dialogFlowBilder.Build();
            #endregion

            //Без данной строки Бот не инициируется на сервере
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            string jsonFile = System.IO.File.ReadAllText("Constellations.json");
            var constParse = JObject.Parse(jsonFile)["Созвездия"].ToArray();
            //данная позиция нужна для метода Substring
            byte fromIndex = 6;
            //данная позиция нужна для метода Substring
            byte numberOfSymbol = 1;

            foreach (var letters in constParse)
            {
                List<Constellation> constellations = new List<Constellation>();
                string letterToString = letters.ToString();
                string letter = letterToString.Substring(fromIndex, numberOfSymbol);
                foreach (var constell in letters[letter])
                {
                    Constellation temp = new Constellation(constell["Name"].ToString(), constell["LatinName"].ToString(),
                                                           constell["UrlMap"].ToString(), constell["UrlPhoto"].ToString(),
                                                           constell["Text"].ToString());

                    constellations.Add(temp);
                }
                LettersConstellations temp2 = new LettersConstellations(letter, constellations);
                LettersConstellations.AlphabetConstellations.Add(temp2);
            }
            
            bot.OnMessage += BotOnMessage;
            bot.OnCallbackQuery += BotOnCallbackQuery;
            bot.StartReceiving();
            var iAm = bot.GetMeAsync().Result;
            ListBoxUserList = new ObservableCollection<UserLog>()
            {
                new UserLog(1111111111, "test1"),
                new UserLog(2222222222, "test2"),
                new UserLog(3333333333, "test3"),
                new UserLog(4444444444, "test4"),
                new UserLog(5555555555, "test5"),
                new UserLog(6666666666, "test6"),
                new UserLog(7777777777, "test7")
            };
            ListBoxUsersBotLogsCmd = new ObservableCollection<UserLog>();
            Choise.ItemsSource = ListBoxUserList;
            CMD.ItemsSource = ListBoxUsersBotLogsCmd;
            UsersList.ItemsSource = ListBoxUserList;

            //Console.ReadLine();
            
            //bot.StopReceiving();
        }

        [Obsolete]
        private static async void BotOnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            string response = e.CallbackQuery.Data;

            //в первое условие попадут данные из метода RequestFiles.ProcessRequestFiles() 
            if (response.Contains("."))
            {
                string[] a = response.Split('.');
                int indexFileName = int.Parse(a[1]);
                string type = a[0];
                string fileName = RequestFiles.FileListName[indexFileName].ToString();

                SendFiles(Path + $@"\{Message.From.FirstName}_{Message.Chat.Id}\{type}\{fileName}", $"{fileName}", type);
                DataToMainWindow(ListBoxUsersBotLogsCmd, $"Отправлен файл {fileName}");
                return;
            }
            //во второе условие приходят данные для дальнейшего поиска нужных типов сохраненных файлов
            if (response == MessageType.Document.ToString() || response == MessageType.Video.ToString() ||
                response == MessageType.Photo.ToString() || response == MessageType.Audio.ToString())
                try
                {
                    RequestFiles.ProcessRequestFiles(response, Message.From.FirstName);
                }
                catch (Exception)
                {
                    throw;
                }
            //ответ пользователю по запросу о созвездиях
            else
            {
                string[] a = response.Split('|');
                string letter = a[0];
                string constellation = a[1];
                for (int i = 0; i < LettersConstellations.AlphabetConstellations.Count; i++)
                {
                    if (letter == LettersConstellations.AlphabetConstellations[i].Letter)
                    {
                        List<Constellation> temp = LettersConstellations.AlphabetConstellations[i].Constellations;
                        foreach (var item in temp)
                        {
                            if (constellation == item.Name)
                            {
                                await bot.SendTextMessageAsync(Message.From.Id, item.Name);
                                await bot.SendTextMessageAsync(Message.From.Id, "На латыни - " + item.LatinName);
                                await bot.SendPhotoAsync(Message.From.Id, item.UrlMap);
                                await bot.SendPhotoAsync(Message.From.Id, item.UrlPhoto);
                                await bot.SendTextMessageAsync(Message.From.Id, item.Text);
                                DataToMainWindow(ListBoxUsersBotLogsCmd, $"Созвездие {item.Name}");
                            }
                        }
                    }
                }
            }
        }


        [Obsolete]
        private static async void BotOnMessage(object sender, MessageEventArgs e)
        {
            Message = e.Message;
            firstName = Message.From.FirstName;
            UserLog tempUser = new UserLog(Message.Chat.Id, firstName);
            
            string answerText = default;

            // передача в основной поток, как работает не знаю, смотрите ДЗ 10.4 на 7 минуте, там ничего не сказано 
            window.Dispatcher.Invoke(() =>
            {
                ListBoxUsersBotLogsCmd.Add(new UserLog(Message.Chat.Id, firstName, Message.Text));
            });


            //создается папка пользователя с подпапками
            if (!Directory.Exists(Path + $@"\{firstName}_{Message.Chat.Id}"))
            {
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}");
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}\Video");
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}\Audio");
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}\Photo");
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}\Document");
            }

            if(ListBoxUserList.All(user => user.Id != tempUser.Id))
            {
                window.Dispatcher.Invoke(() =>
                {
                    ListBoxUserList.Add(new UserLog(Message.Chat.Id, firstName));
                });
            }

            //условие при котором формируются Inline кнопки созвездий согласно буквы
            if (Message.Type == MessageType.Text && Message.Text.Length == 1)
            {
                PrepareConstellations.ReturnInlineKeyboard(Message.Text);
                return;
            }

            switch (Message.Text)
            {
                case "/start":
                    answerText = @"Бот может общаться, может сохранять и показывать переданные ему файлы, " +
                                 "может рассказать Вам о созвездиях.\n" +
                                 "Используйте следующие команды:\n" +
                                 "/menu - выбор Меню\n";
                    await bot.SendTextMessageAsync(Message.From.Id, answerText);
                    break;
                case "/menu":
                    var keyboardButtons = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton("Инф. о созвездиях"),
                        new KeyboardButton("Сохраненные файлы")
                    }, resizeKeyboard: true);
                    await bot.SendTextMessageAsync(Message.Chat.Id, "Выбран пункт 'menu'", replyMarkup: keyboardButtons);
                    break;
                case "Сохраненные файлы":
                    var keyboardInline = new InlineKeyboardMarkup(new[]
                    {
                       new[]
                       {
                           InlineKeyboardButton.WithCallbackData("Аудио файлы", "Audio"),
                           InlineKeyboardButton.WithCallbackData("Видео файлы", "Video"),
                       },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Документы", "Document"),
                            InlineKeyboardButton.WithCallbackData("Фото", "Photo")
                        }
                    });
                    await bot.SendTextMessageAsync(Message.From.Id, "Укажите тип файлов на клавитатуре", replyMarkup: keyboardInline);
                    break;
                case "Инф. о созвездиях":
                    var alphabetKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            new KeyboardButton("А"),
                            new KeyboardButton("Б"),
                            new KeyboardButton("В"),
                            new KeyboardButton("Г"),
                            new KeyboardButton("Д"),
                            new KeyboardButton("Е"),
                            new KeyboardButton("Ж"),
                            new KeyboardButton("З"),
                            new KeyboardButton("И")
                        },
                        new[]
                        {
                            new KeyboardButton("К"),
                            new KeyboardButton("Л"),
                            new KeyboardButton("М"),
                            new KeyboardButton("Н"),
                            new KeyboardButton("О"),
                            new KeyboardButton("П"),
                            new KeyboardButton("Р"),
                            new KeyboardButton("С"),
                            new KeyboardButton("Т")
                        },
                        new[]
                        {
                            new KeyboardButton("Ф"),
                            new KeyboardButton("Х"),
                            new KeyboardButton("Ц"),
                            new KeyboardButton("Ч"),
                            new KeyboardButton("Щ"),
                            new KeyboardButton("Э"),
                            new KeyboardButton("Ю"),
                            new KeyboardButton("Я"),
                            new KeyboardButton(":)")
                        },
                    }, resizeKeyboard: true);
                    await bot.SendTextMessageAsync(Message.Chat.Id, "Выбран пункт 'Инф. о созвездиях'", replyMarkup: alphabetKeyboard);
                    break;
                case ":)":
                    {
                        await bot.SendTextMessageAsync(Message.Chat.Id, "Рад, что Вам понравилось");
                        await bot.SendTextMessageAsync(Message.Chat.Id, "/menu");
                        break;
                    }
                default:
                    // Общение с ботом через DialogFlow
                    // Инициализируем аргументы ответа
                    if (Message.Type == MessageType.Text) // проверка если вдруг отправят вложение с пустым текстовым полем
                    {
                        SessionName session = SessionName.FromProjectSession(projectID, sessionID);
                        var queryInput = new QueryInput
                        {
                            Text = new TextInput
                            {
                                Text = Message.Text,
                                LanguageCode = "ru-ru"
                            }
                        };

                        // Создаем ответ пользователю
                        DetectIntentResponse response = await dFlowClient.DetectIntentAsync(session, queryInput);

                        answerText = response.QueryResult.FulfillmentText;

                        if (answerText == "")
                        {
                            //answerText = ">:P";
                            return;
                        }
                        await bot.SendTextMessageAsync(Message.Chat.Id, answerText); // отправляем пользователю ответ
                    }
                    break;
            }

            switch (Message.Type)
            {
                case MessageType.Photo:
                    //Console.WriteLine("Получено фото ");
                    DataToMainWindow(ListBoxUsersBotLogsCmd, "Получено фото ");
                    string photoFileId = Message.Photo[Message.Photo.Length - 1].FileId;
                    Download(photoFileId, Path + $@"\{firstName}_{Message.Chat.Id}\Photo\" + (Message.Photo[Message.Photo.Length - 1]).FileUniqueId + ".jpg");
                    await bot.SendTextMessageAsync(Message.From.Id, "я сохранил это");
                    break;
                case MessageType.Document:
                    //Console.WriteLine("Получен документ " + Message.Document.FileName);
                    DataToMainWindow(ListBoxUsersBotLogsCmd, "Получен документ " + Message.Document.FileName);
                   Download(e.Message.Document.FileId, Path + $@"\{firstName}_{Message.Chat.Id}\Document\" + Message.Document.FileName);
                    await bot.SendTextMessageAsync(Message.From.Id, "я сохранил это");
                    break;
                case MessageType.Video:
                    //Console.WriteLine("Получен видео файл " + Message.Video.FileName);
                    DataToMainWindow(ListBoxUsersBotLogsCmd, "Получен видео файл " + Message.Video.FileName);
                    Download(Message.Video.FileId, Path + $@"\{firstName}_{Message.Chat.Id}\Video\" + Message.Video.FileName);
                    await bot.SendTextMessageAsync(Message.From.Id, "я сохранил это");
                    break;
                case MessageType.Audio:
                    //Console.WriteLine("Получен аудио файл " + Message.Audio.FileName);
                    DataToMainWindow(ListBoxUsersBotLogsCmd, "Получен аудио файл " + Message.Audio.FileName);
                    Download(Message.Audio.FileId, Path + $@"\{firstName}_{Message.Chat.Id}\Audio\" + Message.Audio.FileName);
                    await bot.SendTextMessageAsync(Message.From.Id, "я сохранил это");
                    break;
            }

        }

        private void BtnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Id.Text))
                return;
            long chatId = long.Parse(Id.Text);
            string name = Name.Text;
            switch (((Button)sender).Name)
            {
                case "Document":
                    {
                        Process.Start(Path + $@"\{name}_{chatId}\Document");
                        break;
                    }
                case "Audio":
                    {
                        Process.Start(Path + $@"\{name}_{chatId}\Audio");
                        break;
                    }
                case "Photo":
                    {
                        Process.Start(Path + $@"\{name}_{chatId}\Photo");
                        break;
                    }
                case "Video":
                    {
                        Process.Start(Path + $@"\{name}_{chatId}\Video");
                        break;
                    }
            }

        }

        async private void SendMsg_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextToUser) | String.IsNullOrEmpty(Id.Text))
            {
                Messages.Text = "Сообщение пользователю";
                MessageBox.Show("Укажите пользователя и текст сообщение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            await bot.SendTextMessageAsync(long.Parse(Id.Text), TextToUser);
            SendText(TextToUser);
            TextToUser = default;
            Messages.Text = "Сообщение пользователю";
        }

        private void Messages_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextToUser = Messages.Text;
        }

        private void Messages_MouseEnter(object sender, MouseEventArgs e)
        {
            Messages.Clear();
        }

        /// <summary>
        /// отправка данных в MainWindow через Диспетчер из другого потока
        /// </summary>
        /// <param name="messageText">текст принятый от пользователя</param>
        private static void DataToMainWindow(ObservableCollection<UserLog> collection, string messageText)
        {

            window.Dispatcher.Invoke(() =>
            {
                collection.Add(new UserLog(Message.Chat.Id, firstName, messageText));
            });
        }
        /// <summary>
        /// возможность отправки логов в поле ListBox (x:Name="CMD") из обработчиков окна MainWindow.xaml
        /// </summary>
        /// <param name="messageText">передаваемый текст</param>
        /// <returns></returns>
        private void SendText(string messageText)
        {
            var botData = bot.GetMeAsync().Result;
            ListBoxUsersBotLogsCmd.Add(new UserLog(botData.Id, "Admin", messageText));
            CMD.ItemsSource = ListBoxUsersBotLogsCmd;
        }

        private void SaveLog_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(Choise.Text))
            {
                MessageBox.Show("Укажите пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var temp = (Choise.Text).Split(' ', ',', ':', '"');
            создание метода по сераилизации логов
        }     
        
            
        private void Choise_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox temp = (ComboBox)sender;
            Choise.Text = temp.SelectedItem.ToString();
        }

        
    }
    
}
