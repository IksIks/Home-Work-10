namespace WPF_Telegram_Bot
{
    public class Constellation
    {
        public string Name { get; private set; }
        public string LatinName { get; private set; }
        public string UrlMap { get; private set; }
        public string UrlPhoto { get; private set; }
        public string Text { get; private set; }


        public Constellation(string name, string latinName, string urlMap, string urlPhoto, string text)
        {
            Name = name;
            LatinName = latinName;
            UrlMap = urlMap;
            UrlPhoto = urlPhoto;
            Text = text;
        }




    }

}
