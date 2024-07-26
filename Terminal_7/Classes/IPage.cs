using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Terminal_7.Classes
{
    public interface IPage
    {
        void Closing();
        void LoadTheme(string theme);
        void RemoveLast(object obj, NavigationEventArgs e);
        void Reload();
        void AdditionalKeys(object sender, KeyEventArgs e);
    }
}
