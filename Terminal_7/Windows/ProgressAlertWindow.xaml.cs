using System.Threading.Tasks;
using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Terminal_7.Classes;

namespace Terminal_7.Windows
{
    public partial class ProgressAlertWindow : Window
    {
        Random r = new Random();
        //string _theme;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Action action;
        public ProgressAlertWindow()
        {
            InitializeComponent();

           
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            //bar.Value += r.Next(0, 11);
            bar.Value += 10;
        }

        private void bar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bar.Value >= 100) 
            {
                action();
            }
        }

        public ProgressAlertWindow(string title, string message, string theme) : this()
        {
            LoadTheme(theme);
            LoadParams(title, message);

            //KeyDown += (obj, e) =>
            //{
            //    if (e.Key == Key.Enter || e.Key == Key.Escape)
            //        Close();
            //};

            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            //this._theme = theme;
            action = () => {
                dispatcherTimer.Stop();
                var alert = new AlertWindow("Уведомление", "Сообщение отправлено", "Закрыть", theme);
                if (alert.ShowDialog() == false)
                {

                }
                Close();
                
            };
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
    }
}
