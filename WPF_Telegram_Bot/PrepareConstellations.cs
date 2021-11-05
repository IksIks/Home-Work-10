using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace WPF_Telegram_Bot
{
    class PrepareConstellations
    {
        /// <summary>
        /// Поиск созвездий на конкретную букву
        /// </summary>
        /// <param name="letter">первая буква</param>
        /// <returns>массив созвездий на конкректную букву</returns>
        private static List<Constellation> SearchLetter(string letter)
        {
            List<Constellation> constellations = new List<Constellation>();
            foreach (var item in LettersConstellations.AlphabetConstellations)
            {
                if (letter == item.Letter)
                {
                    constellations = item.Constellations;
                    break;
                }
                else constellations = null;
            }
            return constellations;
        }
        /// <summary>
        /// Отображает в боте Inline клавиши созвездий
        /// </summary>
        /// <param name="letter">первая буква на которую начинаются созвездия</param>
        public static async void ReturnInlineKeyboard(string letter)
        {
            List<Constellation> answerSerchLetter = SearchLetter(letter);
            if (answerSerchLetter == null)
            {
                string text = $"Используй специальную клавиатуру, на букву '{letter}' нет созвездий";
                await MainWindow.bot.SendTextMessageAsync(MainWindow.Messages.Chat.Id, text);
            }
            else
            {
                //массив кнопок для того чтоб бот разместил их вертикально
                List<List<InlineKeyboardButton>> constellationsButtons = new List<List<InlineKeyboardButton>>();

                for (int i = 0; i < answerSerchLetter.Count; i++)
                {
                    List<InlineKeyboardButton> buttonArray = new List<InlineKeyboardButton>();
                    InlineKeyboardButton button = new InlineKeyboardButton();
                    button.CallbackData = letter + "|" + answerSerchLetter[i].Name;
                    button.Text = answerSerchLetter[i].Name;
                    buttonArray.Add(button);
                    constellationsButtons.Add(buttonArray);
                }
                InlineKeyboardMarkup markup = new InlineKeyboardMarkup(constellationsButtons);
                await MainWindow.bot.SendTextMessageAsync(MainWindow.Messages.Chat.Id, $"Созвездия на букву '{letter}'", replyMarkup: markup);
            }
        }
    }
}
