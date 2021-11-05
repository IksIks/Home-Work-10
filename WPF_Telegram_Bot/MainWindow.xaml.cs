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

namespace WPF_Telegram_Bot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {


            InitializeComponent();
            int[] a = new int[100];
            Random q = new Random(200000000);
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = q.Next();
            }
            Test.ItemsSource = a;
        }

        private void BtnClick(object sender, RoutedEventArgs e)
        {
            
            switch (((Button)sender).Content)
            {
                case "Документы":
                    {
                        break;
                    }
                case "Аудио":
                    {
                        break;
                    }
                case "Фото":
                    {
                        break;
                    }
                case "Видео":
                    {
                        break;
                    }
            }
        
        }
    }
}
