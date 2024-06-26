﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Terminal_7.Classes;

namespace Terminal_7.Windows.Game
{
    /// <summary>
    /// Логика взаимодействия для SputnikGameWindow.xaml
    /// </summary>
    public partial class SputnikGameWindow : Window
    {
        private Random random = new Random();
        private uint _timer = 0;
        private uint _nowTime = 0;
        private uint _totalTime = 32768;
        private short _dirX = 0;
        private short _dirY = 0;
        private readonly string _theme;


        public SputnikGameWindow(string theme)
        {
            InitializeComponent();
            if (!ConfigManager.Config.IsDebugMode)
            {
                Topmost = true;
                Cursor = Cursors.None;
            }
            else
            {
                Topmost = false;
            }
            _theme = theme;
            Focus();
            Initialize();
            Loaded += (s, e) =>
            {
                Canvas.SetTop(sputnik, ActualHeight / 2 - sputnik.Height / 2);
                Canvas.SetLeft(sputnik, ActualWidth / 2 - sputnik.Width / 2);
                Canvas.SetTop(area, ActualHeight / 2 - area.Height / 2);
                Canvas.SetLeft(area, ActualWidth / 2 - area.Width / 2);
                Canvas.SetTop(timerZone, 50);
                Canvas.SetLeft(timerZone, 50);
            };
            KeyDown += KeyPress;
        }
        private void Initialize()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;

            sputnik.Height = 20;
            sputnik.Width = 20;
            sputnik.Fill = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);

            area.BorderBrush = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
            area.BorderThickness = new Thickness(2);
            area.Height = 400;
            area.Width = 400;

            timerZone.Width = 400;
            timerZone.Height = 100;

            mainField.Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(Addition.Themes + _theme + "/Background.png", UriKind.Relative)) };


            var nameFont = "Font.ttf";
            var fullpath = Path.GetFullPath(Addition.Themes + _theme + "/" + nameFont);

            var families = Fonts.GetFontFamilies(fullpath);
            var family1 = families.First();


            timerZone.FontFamily = family1;
            timerZone.FontSize = ConfigManager.Config.FontSize;
            timerZone.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);

        }
        private void UpdateSputnik()
        {
            Thread.Sleep(1000);
            bool flag = true;
            _timer = 1001;
            double temp = 0;
            while (_nowTime < _totalTime)
            {

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    timerZone.Text = "Передано байт: " + _nowTime.ToString() + "\nОсталось байт: " + (_totalTime - _nowTime).ToString();
                    double sTop = 0;
                    double sLeft = 0;
                    double aTop = 0;
                    double aLeft = 0;

                    sTop = Canvas.GetTop(sputnik);
                    sLeft = Canvas.GetLeft(sputnik);
                    aTop = Canvas.GetTop(area);
                    aLeft = Canvas.GetLeft(area);

                    if (!IsIntersect(sTop, sLeft, aTop, aLeft))
                    {
                        temp = _nowTime;
                        _nowTime = _totalTime + 5;
                        flag = false;
                    }


                    if (sTop < 5 || sTop > ActualHeight - 5 - sputnik.Height || sLeft < 5 || sLeft > ActualWidth - 5 - sputnik.Width)
                    {
                        _dirX *= -1;
                        _dirY *= -1;
                    }

                    if (_timer >= 1000)
                    {
                        switch (random.Next(1, 5))
                        {
                            case 1:
                                _dirX = 1;
                                _dirY = 0;
                                break;
                            case 2:
                                _dirX = 0;
                                _dirY = 1;
                                break;
                            case 3:
                                _dirX = -1;
                                _dirY = 0;
                                break;
                            case 4:
                                _dirX = 0;
                                _dirY = -1;
                                break;
                            default:
                                break;
                        }

                        _timer = 0;
                    }
                    Canvas.SetTop(sputnik, sTop + _dirY * 2);
                    Canvas.SetLeft(sputnik, sLeft + _dirX * 2);
                }));
                //if (flag)
                //{
                //    break;
                //}

                _timer += 10;
                _nowTime += 10;
                Thread.Sleep(10);
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {

                AlertWindow al;
                if (flag)
                {
                    al = new AlertWindow("Уведомление", "Данные со спутника отправленны", "Закрыть", _theme);
                    timerZone.Text = "Передано байт: " + _nowTime.ToString() + "\nОсталось байт: 0";
                }
                else
                {
                    al = new AlertWindow("Уведомление", "Данные со спутника не были отправленны", "Закрыть", _theme);
                    timerZone.Text = "Передано байт: " + temp.ToString() + "\nОсталось байт: " + (_totalTime - temp).ToString();
                }

                if (al.ShowDialog() == false)
                {
                    _timer = _totalTime + 5;
                    Close();
                }
            }));



        }
        private bool IsIntersect(double sTop, double sLeft, double aTop, double aLeft)
        {
            if (sTop > aTop && sTop + sputnik.Height < aTop + area.Height && sLeft > aLeft && sLeft + sputnik.Width < aLeft + area.Width)
            {
                return true;
            }
            return false;
        }
        private void KeyPress(object sender, KeyEventArgs e)
        {
            double sdvig = 10;
            switch (e.Key)
            {
                case Key.Escape:
                    //ReternedState = State.Cancel;
                    _timer = _totalTime + 5;
                    Close();
                    break;
                case Key.D:
                case Key.Right:
                    Canvas.SetLeft(area, Canvas.GetLeft(area) + sdvig);
                    break;
                case Key.A:
                case Key.Left:
                    Canvas.SetLeft(area, Canvas.GetLeft(area) - sdvig);
                    break;
                case Key.S:
                case Key.Down:
                    Canvas.SetTop(area, Canvas.GetTop(area) + sdvig);
                    break;
                case Key.W:
                case Key.Up:
                    Canvas.SetTop(area, Canvas.GetTop(area) - sdvig);
                    break;
                case Key.Enter:
                    Thread updateSputnik = new Thread(UpdateSputnik);
                    updateSputnik.Start();
                    break;
            }
        }
    }
}
