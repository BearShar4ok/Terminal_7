﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Terminal_7.Frames;
using Terminal_7.Classes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Terminal_7.Windows.Game;
using Terminal_7.Windows;
using Terminal_7.Frames.ViewPages;

namespace Terminal_7
{
    public partial class MainWindow : Window
    {
        private readonly string _theme;

        public MainWindow()
        {

            InitializeComponent();




            _theme = ConfigManager.Config.Theme;

            if (!ConfigManager.Config.IsDebugMode)
            {
                Topmost = true;
                Cursor = Cursors.None;
            }
            else
            {
                Topmost = false;
            }

            LoadTheme(_theme);

            LoadParams();

            //LabirintGameWindow lw = new LabirintGameWindow(_theme);
            //if (lw.ShowDialog() != false)
            //{
            //
            //}
            //Close();

            //ProgressAlertWindowText pw = new ProgressAlertWindowText("Отправка файлов", "Ожидайте. Идет отправка...", _theme);
            //if (pw.ShowDialog() == false)
            //{
            //
            //}
            //Close();
        }

        private void LoadTheme(string name)
        {
            Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(Addition.Themes + name + "/Background.png", UriKind.Relative)) };
        }

        private void LoadParams()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            Closing += (obj, e) => DevicesManager.StopListening();

            Addition.NavigationService?.Navigate(new TechnicalViewPage(_theme, new LoadingPage(_theme)));


        }

        private void TryExecuteMethod(object obj, Type type, string name)
        {
            try
            {
                foreach (var item in type.GetMethods())
                {
                    if (item.Name == name)
                        item.Invoke(obj, default);
                }
            }
            catch { }
        }
    }
}
