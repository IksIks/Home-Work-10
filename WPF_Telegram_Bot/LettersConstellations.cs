using System.Collections.Generic;

namespace WPF_Telegram_Bot
{
    class LettersConstellations
    {
        //список созведий по алфавиту
        public static List<LettersConstellations> AlphabetConstellations = new List<LettersConstellations>();
        public string Letter { get; private set; }
        public List<Constellation> Constellations { get; private set; }
        public LettersConstellations(string letter, List<Constellation> constellations)
        {
            Letter = letter;
            Constellations = constellations;
        }
    }
}
