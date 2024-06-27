using System;
using System.IO;
using System.Linq;
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
            var nameFont = "Font.ttf";
            var fullpath = Path.GetFullPath(Addition.Themes + theme + "/" + nameFont);

            var families = Fonts.GetFontFamilies(fullpath);
            var family1 = families.First();

            LblTitle.FontFamily = family1;
            TbMessage.FontFamily = family1;
            LblButton.FontFamily = family1;

            button.Background = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColorSecond);

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
