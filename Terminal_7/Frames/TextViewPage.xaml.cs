﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Terminal_7.Classes;
using Terminal_7.Windows;

namespace Terminal_7.Frames
{
    public partial class TextViewPage : Page
    {
        protected string _filename;
        protected string _theme;
        protected bool _update;
        protected Mutex _mutex = new Mutex();
        protected bool _isItCommand;
        protected int _caretPos;

        public static RoutedCommand SaveFileCommand = new RoutedCommand();
        public static RoutedCommand SendFileCommand = new RoutedCommand();

        ConfigDeserializer permitions;

        public TextViewPage(string filename, string theme, bool clearPage = false, bool isItCommand = false)
        {
            InitializeComponent();

            if (clearPage)
                Addition.NavigationService.Navigated += RemoveLast;

            LoadTheme(theme);


            _filename = filename;
            _theme = theme;
            _isItCommand = isItCommand;
            Output.Text = ConfigManager.Config.SpecialSymbol;

            LoadParams();

            Application.Current.MainWindow.KeyDown += AdditionalKeys;
            Scroller.Focus();
            LoadText();
        }
        protected void RemoveLast(object obj, NavigationEventArgs e)
        {
            Addition.NavigationService?.RemoveBackEntry();
        }

        public void Closing()
        {
            _update = false;
            Addition.NavigationService.Navigated -= RemoveLast;
            Application.Current.MainWindow.KeyDown -= AdditionalKeys;
        }

        public void Reload()
        {
            ConfigManager.Load();
            LoadParams();
            LoadTheme(_theme);

            _update = false;

            _mutex?.WaitOne();
            Output.Text = ConfigManager.Config.SpecialSymbol;
            _mutex?.ReleaseMutex();

            LoadText();
        }

        protected void LoadText()
        {
            if (!File.Exists(_filename))
                return;

            _update = true;

            new Thread(() =>
            {
                using (var stream = File.OpenText(_filename))
                {
                    var text = stream.ReadToEnd();
                    _caretPos = text.Length;
                    if (_isItCommand)
                    {
                        var t = text.Split('\n').ToList();
                        RequestSender.SendGet(t[0].Replace("\r", ""));
                        t.RemoveAt(0);
                        text = string.Join("\n", t.ToArray());
                    }

                    Addition.PrintLines(Output, Scroller, Dispatcher, ref _update, _mutex,
                        new FragmentText(text,
                            ConfigManager.Config.UsingDelayFastOutput ? ConfigManager.Config.DelayFastOutput : 0));
                    Dispatcher.BeginInvoke(DispatcherPriority.Background,
                     new Action(() =>
                     {
                         Output.Text = Output.Text.Remove(Output.Text.Length - 1, 1);
                         if (Output.Text.Length > 0)
                             Output.CaretIndex = Output.Text.Length - 1;
                         else
                             Output.CaretIndex = 0;
                     }));

                    UpdateCarriage();
                }
            }).Start();
        }

        protected void LoadTheme(string theme)
        {
            var nameFont = "Font.ttf";
            var fullpath = Path.GetFullPath(Addition.Themes + theme + "/" + nameFont);

            var families = Fonts.GetFontFamilies(fullpath);
            var family1 = families.First();
            Output.FontFamily = family1;
        }

        protected void LoadParams()
        {
            Output.FontSize = ConfigManager.Config.FontSize;
            Output.Opacity = ConfigManager.Config.Opacity;
            Output.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
            Output.Background = new SolidColorBrush(Colors.Transparent);
            Output.BorderThickness = new Thickness(0);
            Output.BorderBrush = new SolidColorBrush(Colors.Transparent);
            Output.Cursor = Cursors.None;
            Output.Focusable = true;
            Output.CaretBrush = new SolidColorBrush(Colors.Transparent);



            if (Directory.GetFiles(_filename.RemoveLast(@"\")).Contains(_filename + ".config"))
            {

                permitions = JsonConvert.DeserializeObject<ConfigDeserializer>(File.ReadAllText(_filename + ".config"));
                if (!permitions.CanBeChanged)
                {
                    Output.IsReadOnly = true;
                }
            }
            SaveFileCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            SendFileCommand.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Control));

            // Move custom caret whenever the selection has changed. (this includes typing, arrow keys, clicking)
            //
            Output.SelectionChanged += (sender, e) => MoveCustomCaret();

            // Keep custom caret collpased until the text box has gained focus
            //
            Output.LostFocus += (sender, e) => CaretCanvas.Visibility = Visibility.Collapsed;

            // Show custom caret as soon as text box has gained focus
            //
            Output.GotFocus += (sender, e) => CaretCanvas.Visibility = Visibility.Visible;
            CaretCanvas.Height = ConfigManager.Config.FontSize;
            CaretCanvas.Width = ConfigManager.Config.FontSize / 2;


        }
        private void MoveCustomCaret()
        {
            var caretLocation = Output.GetRectFromCharacterIndex(Output.CaretIndex).Location;

            if (!double.IsInfinity(caretLocation.X))
            {
                Canvas.SetLeft(CaretCanvas, caretLocation.X);
            }

            if (!double.IsInfinity(caretLocation.Y))
            {
                Canvas.SetTop(CaretCanvas, caretLocation.Y);
            }
        }
        protected void UpdateCarriage()
        {

            new Thread(() =>
            {
                bool flag = true;

                while (_update)
                {
                    _mutex?.WaitOne();
                    Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() =>
                    {

                        if (flag)
                            CaretCanvas.Background = new SolidColorBrush(Colors.Transparent);
                        else
                            CaretCanvas.Background = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
                        flag = !flag;
                    }));

                    _mutex?.ReleaseMutex();

                    Thread.Sleep((int)ConfigManager.Config.DelayUpdateCarriage);

                }
            }).Start();
        }
        private void SaveFile(object sender, ExecutedRoutedEventArgs e)
        {
            File.WriteAllText(_filename, Output.Text);
        }
        private void SendFile(object sender, ExecutedRoutedEventArgs e)
        {
            ProgressAlertWindowText pw = new ProgressAlertWindowText("Отправка файлов", "Ожидайте. Идет отправка...", _theme);
            if (pw.ShowDialog() == false)
            {

            }

            var alert = new AlertWindow("Уведомление", "Сообщение отправлено", "Закрыть", _theme);
            if (alert.ShowDialog() == false)
            {

            }
        }

        protected void AdditionalKeys(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Closing();
                    Addition.NavigationService.GoBack();
                    break;
                case Key.Left:
                    _caretPos--;
                    break;
                case Key.Enter:
                    if (Output.IsReadOnly)
                        break;
                    int temppos = Output.CaretIndex;
                    Output.Text = Output.Text.Insert(Output.CaretIndex, "\r\n");
                    Output.CaretIndex = temppos + 1;
                    break;
                case Key.Right:
                    _caretPos++;
                    break;
            }
        }

        private void Output_KeyDown(object sender, KeyEventArgs e)
        {
            if (!Output.IsReadOnly)
                return;
            if (e.Key == Key.Escape)
                return;

            var alert = new AlertWindow("Уведомление", "Недостаточно прав для редактирования", "Закрыть", _theme);
            if (alert.ShowDialog() == false)
            {

            }
        }
    }
}