using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Google.Cloud.Dialogflow.V2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Win32;



namespace WPF_Telegram_Bot
{
    public partial class MainWindow : Window
    {
        #region Поля и свойства класса

        #region Поля
        public static TelegramBotClient bot;
        private static string firstName = default;
        private static SessionsClient dFlowClient;
        private static string projectID;
        private static string sessionID;
        private static string path;
        #endregion

        #region Свойства
        // свойство для передачи в ListBox "UserList"
        private static ObservableCollection<UserLog> UserListListBox { get; set; }
        // свойство для записи логов всех подключаемых пользователей и передачи в ListBox "CMD"
        public static ObservableCollection<UserLog> UsersBotCmdListBox { get; set; }

        //переменная для возможности передачи в основной поток
        private static MainWindow window { get; set; }

        // свойство для записи вводимого администратором текста для дальнейшей отправки его выбранному пользователю
        private string TextToUser { get; set; }


        public static Telegram.Bot.Types.Message Message { get; set; }

        //Начальный путь к каталогу
        public static string Path
        {
            get { return @"c:\Telegram_Bot_Users"; }
            private set { path = value; }
        }
        #endregion

        #endregion

        [Obsolete]
        public MainWindow()
        {
            InitializeComponent();
            string tokenBot = default;
            window = this;

            // показ диалогового окна для ввода токена бота
            if (MessageBox.Show("У Вас есть Token бота?", "Уведомление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "Выбор файла Token";
                dialog.Filter = "Файлы(*.txt)|*.txt";
                dialog.ShowDialog();
                tokenBot = System.IO.File.ReadAllText(dialog.FileName) ?? String.Empty;
            }
            else System.Environment.Exit(0); // заврешение программы при отказе ввода токена

            bot = new TelegramBotClient(tokenBot);

            // проверка на валидность токена, если не прошла - программа ловит исключение и закрывается
            try
            {
                var iAm = bot.GetMeAsync().Result;
            }
            catch (Exception)
            {
                MessageBox.Show("TOKEN_ERROR, sory", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Environment.Exit(0);
            }
            
            bot.OnMessage += BotOnMessage;
            bot.OnCallbackQuery += BotOnCallbackQuery;
            bot.StartReceiving();

            string dFlowKeyPath = @"jsonFiles\iksbot-9tan-8bfc6cdbd2be.json";
            #region BotAnswerInitiation

            var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(dFlowKeyPath));
            projectID = dic["project_id"];
            sessionID = dic["private_key_id"];
            var dialogFlowBilder = new SessionsClientBuilder { CredentialsPath = dFlowKeyPath };
            dFlowClient = dialogFlowBilder.Build();

            #endregion
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            //Без данной строки Бот не инициируется на сервере

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            string jsonFile = System.IO.File.ReadAllText("jsonFiles\\Constellations.json");
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
            
           
            UserListListBox = new ObservableCollection<UserLog>();
            UsersBotCmdListBox = new ObservableCollection<UserLog>();
            Choise.ItemsSource = UserListListBox;
            CMD.ItemsSource = UsersBotCmdListBox;
            UsersList.ItemsSource = UserListListBox;
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
                DataToMainWindow(UsersBotCmdListBox, $"Отправлен файл {fileName}");
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
                                DataToMainWindow(UsersBotCmdListBox, $"Созвездие {item.Name}");
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
                UsersBotCmdListBox.Add(new UserLog(Message.Chat.Id, firstName, Message.Text, Message.Date));
            });


            //создается папка пользователя с подпапками
            if (!Directory.Exists(Path + $@"\{firstName}_{Message.Chat.Id}"))
            {
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}");
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}\Video");
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}\Audio");
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}\Photo");
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}\Document");
                Directory.CreateDirectory(Path + $@"\{firstName}_{Message.Chat.Id}\Logs");
            }

            if(UserListListBox.All(user => user.Id != tempUser.Id))
            {
                window.Dispatcher.Invoke(() =>
                {
                    UserListListBox.Add(new UserLog(Message.Chat.Id, firstName));
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
                    DataToMainWindow(UsersBotCmdListBox, "Получено фото ");
                    string photoFileId = Message.Photo[Message.Photo.Length - 1].FileId;
                    Download(photoFileId, Path + $@"\{firstName}_{Message.Chat.Id}\Photo\" + (Message.Photo[Message.Photo.Length - 1]).FileUniqueId + ".jpg");
                    await bot.SendTextMessageAsync(Message.From.Id, "я сохранил это");
                    break;
                case MessageType.Document:
                    //Console.WriteLine("Получен документ " + Message.Document.FileName);
                    DataToMainWindow(UsersBotCmdListBox, "Получен документ " + Message.Document.FileName);
                   Download(e.Message.Document.FileId, Path + $@"\{firstName}_{Message.Chat.Id}\Document\" + Message.Document.FileName);
                    await bot.SendTextMessageAsync(Message.From.Id, "я сохранил это");
                    break;
                case MessageType.Video:
                    //Console.WriteLine("Получен видео файл " + Message.Video.FileName);
                    DataToMainWindow(UsersBotCmdListBox, "Получен видео файл " + Message.Video.FileName);
                    Download(Message.Video.FileId, Path + $@"\{firstName}_{Message.Chat.Id}\Video\" + Message.Video.FileName);
                    await bot.SendTextMessageAsync(Message.From.Id, "я сохранил это");
                    break;
                case MessageType.Audio:
                    //Console.WriteLine("Получен аудио файл " + Message.Audio.FileName);
                    DataToMainWindow(UsersBotCmdListBox, "Получен аудио файл " + Message.Audio.FileName);
                    Download(Message.Audio.FileId, Path + $@"\{firstName}_{Message.Chat.Id}\Audio\" + Message.Audio.FileName);
                    await bot.SendTextMessageAsync(Message.From.Id, "я сохранил это");
                    break;
            }

        }

        #region Методы

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

        /// <summary>
        /// открытие в explorer соотвествующей папки пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// отправка ВЫБРАННОМУ пользоваелю сообщения от имени администратора
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void SendMsg_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextToUser) | String.IsNullOrEmpty(Id.Text))
            {
                MessageBox.Show("Укажите пользователя и текст сообщение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            await bot.SendTextMessageAsync(long.Parse(Id.Text), TextToUser);
            SendText(TextToUser);
            Messages.Clear();
        }

        /// <summary>
        /// передача введенного текста из TextBox "Messages"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Messages_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextToUser = Messages.Text;
        }

        /// <summary>
        /// отправка данных в MainWindow через Диспетчер из другого потока
        /// </summary>
        /// <param name="messageText">текст принятый от пользователя</param>
        public static void DataToMainWindow(ObservableCollection<UserLog> collection, string messageText)
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
            UsersBotCmdListBox.Add(new UserLog(botData.Id, $"Admin -> {Message.From.Id} {Message.From.FirstName}:", messageText));
            CMD.ItemsSource = UsersBotCmdListBox;
        }

        /// <summary>
        /// сохранение логов выбранного пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveLog_Click(object sender, RoutedEventArgs e)
        {
            // путь к папке Log текущего пользователя
            if (String.IsNullOrEmpty(Choise.Text))
            {
                MessageBox.Show("Укажите пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string pathForLog = Path + $@"\{Message.From.FirstName}_{Message.Chat.Id}\Logs\log_{DateTime.Now.ToShortDateString()}.json";
            
            SerializerLog SerializeUserMessages = new SerializerLog();
            if (!File.Exists(pathForLog))
                File.WriteAllText(pathForLog, SerializeUserMessages.Serialize(UsersBotCmdListBox
                                                                              ,Message.Chat.Id
                                                                              ,Message.From.FirstName));
            // дозапись последних сообщений в Log пользователя за текущую дату
            else File.AppendAllText(pathForLog, SerializeUserMessages.Serialize(UsersBotCmdListBox
                                                                               ,Message.Chat.Id
                                                                               ,File.GetLastWriteTime(pathForLog)));
            MessageBox.Show($"Сообщения пользователя {Message.From.FirstName} сохранены", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        /// <summary>
        /// сохранение всех логов всех пользователей по папкам
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAllLog_Click(object sender, RoutedEventArgs e)
        {
                if (UserListListBox.Count == 0)
                {
                    MessageBox.Show("Нет подключенных пользователей", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            foreach (var user in UserListListBox)
            {
                string pathForLog = Path + $@"\{Message.From.FirstName}_{Message.Chat.Id}\Logs\log_{DateTime.Now.ToShortDateString()}.json";
                SerializerLog SerializeUserMessages = new SerializerLog();
                if (!File.Exists(pathForLog))
                    File.WriteAllText(pathForLog, SerializeUserMessages.Serialize(UsersBotCmdListBox
                                                                                  , Message.Chat.Id
                                                                                  , Message.From.FirstName));
                // дозапись последних сообщений в Log пользователя за текущую дату
                else File.AppendAllText(pathForLog, SerializeUserMessages.Serialize(UsersBotCmdListBox
                                                                                   , Message.Chat.Id
                                                                                   , File.GetLastWriteTime(pathForLog)));
                MessageBox.Show($"Сообщения всех пользователей сохранены", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion
    }
}
