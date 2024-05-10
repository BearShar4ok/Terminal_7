using System.Linq;
using System.Windows;
using System.Windows.Media;
using Terminal_7.Classes;

namespace Terminal_7
{
    public partial class App : Application
    {
        public App()
        {
            ConfigManager.Load();
            var colorInChar = ConfigManager.Config.TerminalColorSecond.ToString().ToArray();
            colorInChar[1] = '7';
            colorInChar[2] = 'F';
            string fill = "";

            foreach (var item in colorInChar)
            {
                fill += item;
            }
            App.Current.Resources.Add("theColorFill", (Brush)new BrushConverter().ConvertFromString(fill));
            App.Current.Resources.Add("theColorBorder", (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColorSecond));
        }
    }
}
