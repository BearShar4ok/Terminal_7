using System.Threading.Tasks;
using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Terminal_7.Classes;
using System.Windows.Controls;
using System.Threading;

namespace Terminal_7.Windows
{
    // 10 cимволов. на 1 символ от 0 до 5 сек ||||||||||||||||||||||||||||||||||||||||||||||||||
    //                                        ---------------------------------------
    public partial class ProgressAlertWindow : Window
    {
        Random r = new Random();
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        bool isWait = false;
        public ProgressAlertWindow()
        {
            InitializeComponent();
        }
        public ProgressAlertWindow(string title, string message, string theme) : this()
        {
            LoadTheme(theme);
            LoadParams(title, message);

            dispatcherTimer.Tag = 0;
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!isWait)
                bar.Value += r.Next(0,11);
            else
                dispatcherTimer.Tag = (int)(dispatcherTimer.Tag) + 10;

            if (isWait && (int)(dispatcherTimer.Tag) >= 10)
            {
                dispatcherTimer.Stop();

                Close();
            }
        }

        private void bar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bar.Value >= 100) 
            {
               isWait = true;
            }
        }

        private void LoadTheme(string theme)
        {
            LblTitle.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#" + ConfigManager.Config.FontName);
            TbMessage.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#" + ConfigManager.Config.FontName);
           // LblButton.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#" + ConfigManager.Config.FontName);

            Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(Addition.Themes + theme + "/Alert_background.png", UriKind.Relative)) };
        }

        private void LoadParams(string title, string message)
        {
            Left = SystemParameters.PrimaryScreenWidth / 2 - Width / 2;
            Top = SystemParameters.PrimaryScreenHeight / 2 - Height / 2;

            Topmost = true;

            LblTitle.Content = title;
            TbMessage.Text = message;
            //LblButton.Content = button;

            LblTitle.FontSize = ConfigManager.Config.FontSize;
            LblTitle.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);

            TbMessage.FontSize = ConfigManager.Config.FontSize;
            TbMessage.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);

           // LblButton.FontSize = ConfigManager.Config.FontSize;
            //LblButton.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
        }

        private void bar_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }
    }
}
