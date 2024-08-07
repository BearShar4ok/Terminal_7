﻿using System.Threading.Tasks;
using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Terminal_7.Classes;
using System.Windows.Controls;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

namespace Terminal_7.Windows
{
    // 10 cимволов. на 1 символ от 0 до 5 сек ||||||||||||||||||||||||||||||||||||||||||||||||||
    //                                        ---------------------------------------
    public partial class ProgressAlertWindowText : Window
    {
        Random r = new Random();
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        bool isWait = false;
        bool isRandomLoading;
        int waitTime;
        int nowTime = 0;
        int maxLen;
        int indexCount = 0;
        int totalTimeLen = 0;

        int[] randomDelays;
        int currentDelayIndex = 0;

        public ProgressAlertWindowText()
        {
            InitializeComponent();
        }
        public ProgressAlertWindowText(string title, string message, int minutes, int seconds, bool isRandomLoading, string theme) : this()
        {
            LoadTheme(theme);
            LoadParams(title, message);

            this.isRandomLoading = isRandomLoading;

            dispatcherTimer.Tag = 0;
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            totalTimeLen = minutes * 60 + seconds;

            dispatcherTimer.Start();
        }
        private void InitializeRandomDelays()
        {
            randomDelays = new int[maxLen+1];
            int remainingDuration = totalTimeLen * 1000;

            for (int i = 0; i < maxLen; i++)
            {
                randomDelays[i] = r.Next(5, remainingDuration / (maxLen - i));
                remainingDuration -= randomDelays[i];
            }

            randomDelays[maxLen ] = remainingDuration;
            ShuffleArray(randomDelays);
        }

        private void ShuffleArray(int[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = r.Next(0, i + 1);
                int temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (isRandomLoading)
                RandomLoading();
            else
                SmoothLoading();

            if (indexCount > maxLen)
            {
                dispatcherTimer.Stop();
                Thread newThread = new Thread(() =>
                {
                    Thread.Sleep(1000);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {

                        Close();
                    }));
                });
                newThread.Start();
            }
        }
        private void SmoothLoading()
        {
            indexCount++;
            ProgresBar.Text = "";
            for (int i = 0; i < indexCount; i++)
            {
                ProgresBar.Text += "|";
            }
            for (int i = indexCount; i <= maxLen; i++)
            {
                ProgresBar.Text += ".";
            }
        }
        private void RandomLoading()
        {
            if (currentDelayIndex < randomDelays.Length)
            {
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds(randomDelays[currentDelayIndex]);
                currentDelayIndex++;

                indexCount++;
                ProgresBar.Text = "";
                for (int i = 0; i < indexCount; i++)
                {
                    ProgresBar.Text += "|";
                }
                for (int i = indexCount; i <= maxLen; i++)
                {
                    ProgresBar.Text += ".";
                }
            }
        }
        private void LoadTheme(string theme)
        {
            var nameFont = "Font.ttf";
            var fullpath = Path.GetFullPath(Addition.Themes + theme + "/" + nameFont);

            var families = Fonts.GetFontFamilies(fullpath);
            var family1 = families.First();


            LblTitle.FontFamily = family1;
            TbMessage.FontFamily = family1;
            ProgresBar.FontFamily = family1;
            Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(Addition.Themes + theme + "/Alert_background.png", UriKind.Relative)) };
        }

        private void LoadParams(string title, string message)
        {
            Left = SystemParameters.PrimaryScreenWidth / 2 - Width / 2;
            Top = SystemParameters.PrimaryScreenHeight / 2 - Height / 2;

            Topmost = true;

            ProgresBar.FontSize = ConfigManager.Config.FontSize;
            ProgresBar.Opacity = ConfigManager.Config.Opacity;
            ProgresBar.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);

            LblTitle.Content = title;
            TbMessage.Text = message;
            //LblButton.Content = button;

            LblTitle.FontSize = ConfigManager.Config.FontSize;
            LblTitle.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);

            TbMessage.FontSize = ConfigManager.Config.FontSize;
            TbMessage.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            maxLen = (int)(ProgresBar.ActualWidth / Math.Ceiling(GetSizeContent(ProgresBar, ".").Width));
            ProgresBar.Text = new string('.', maxLen);
            float timeTick = (float)totalTimeLen / (float)maxLen;

            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(timeTick * 1000));
            InitializeRandomDelays();
        }
        private static Size GetSizeContent(TextBlock tb, string content) => MeasureString(content, tb.FontFamily, tb.FontStyle,
           tb.FontWeight, tb.FontStretch, tb.FontSize);

        private static Size MeasureString(string candidate, FontFamily font, FontStyle style, FontWeight weight, FontStretch stretch, double fontsize)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(font, style, weight, stretch),
                fontsize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}
