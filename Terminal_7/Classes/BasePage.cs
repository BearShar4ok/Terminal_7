using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows;

namespace Terminal_7.Classes
{
    public class BasePage
    {
        protected string _theme;
        protected string _filename;
        public virtual void Closing()
        {
            Addition.NavigationService.Navigated -= RemoveLast;
            Application.Current.MainWindow.PreviewKeyDown -= AdditionalKeys;
        }
        public virtual void LoadTheme(string theme)
        {

        }
        public virtual void RemoveLast(object obj, NavigationEventArgs e)
        {
            Addition.NavigationService?.RemoveBackEntry();
        }
        public virtual void Reload()
        {
            LoadTheme(_theme);
            LoadParams();
        }
        public virtual void AdditionalKeys(object sender, KeyEventArgs e)
        {

        }
        public virtual void LoadParams()
        {

        }
    }
}
