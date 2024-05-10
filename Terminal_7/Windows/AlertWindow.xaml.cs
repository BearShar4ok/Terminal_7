using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Terminal_7.Classes;

namespace Terminal_7.Windows
{
    /// <summary>
    /// Логика взаимодействия для AlertWindow.xaml
    /// </summary>
    public partial class AlertWindow : Window
    {
        public AlertWindow()
        {
            InitializeComponent();
        }

        public AlertWindow(string title, string message, string button, string theme) : this()
        {
            LoadTheme(theme);
            LoadParams(title, message, button);

            KeyDown += (obj, e) =>
            {
                if (e.Key == Key.Enter || e.Key == Key.Escape)
                    Close();
            };
        }

        private void LoadTheme(string theme)
        {
            LblTitle.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#" + ConfigManager.Config.FontName);
            TbMessage.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#" + ConfigManager.Config.FontName);
            LblButton.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Addition.Themes + theme + "/#" + ConfigManager.Config.FontName);

            Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(Addition.Themes + theme + "/Alert_background.png", UriKind.Relative)) };
        }

        private void LoadParams(string title, string message, string button)
        {
            Left = SystemParameters.PrimaryScreenWidth / 2 - Width / 2;
            Top = SystemParameters.PrimaryScreenHeight / 2 - Height / 2;

            Topmost = true;

            LblTitle.Content = title;
            TbMessage.Text = message;
            LblButton.Content = button;

            LblTitle.FontSize = ConfigManager.Config.FontSize;
            LblTitle.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);

            TbMessage.FontSize = ConfigManager.Config.FontSize;
            TbMessage.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);

            LblButton.FontSize = ConfigManager.Config.FontSize;
            LblButton.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
        }
    }
}
