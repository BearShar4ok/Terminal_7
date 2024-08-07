﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Terminal_7.Frames;
using Terminal_7.Windows.Game;
using Terminal_7.Frames.ViewPages;

namespace Terminal_7.Classes
{
    public static class Addition
    {
        // Path to directory with themes 
        public const string Themes = "Assets/Themes/";
        // Path to icons folder in theme
        public const string Icons = "Icons";
        // Path to labirintGame folder in theme
        public const string LabirintGame = "LabirintGame";
        // Path to directory with assets 
        public const string Assets = "Assets";
        // Path to directory with error.log file 
        public const string ErrorFile = "files/Error.log";

        // NavigationService
        public static NavigationService NavigationService { get; } = (Application.Current.MainWindow as MainWindow)?.Frame.NavigationService;

        // Get files types
        public static readonly string[] Audio = { "wav", "m4a", "mp3", "aac", "flac" };
        public static readonly string[] Image = { "jpeg", "png", "jpg", "tiff", "bmp" };
        public static readonly string[] Video = { "mp4", "gif", "wmv", "avi" };
        public static readonly string[] Text = { "txt" };
        public static readonly string[] Command = { "command" };
        public static readonly string[] Execute = { "execute" };

        public static void ForEach<T>(this IEnumerable<T> lst, Action<T> action)
        {
            if (action == null)
                return;

            foreach (var item in lst)
            {
                action.Invoke(item);
            }
        }

        public static J Pop<T, J>(this Dictionary<T, J> dct, T key)
        {
            if (!dct.ContainsKey(key))
                return default;

            var item = dct[key];
            dct.Remove(key);

            return item;
        }

        public static T FindKey<T, J>(this Dictionary<T, J> dct, J val) =>
            dct.Keys.FirstOrDefault(key => dct[key].Equals(val));

        public static string RemoveLast(this string str, string key) => str.Remove(str.LastIndexOf(key, StringComparison.Ordinal));

        // Method to print list string to textblock with delay
        public static void PrintLines(TextBlock element, ScrollViewer scroller, Dispatcher dispatcher, ref bool working, Mutex mutex = default, params FragmentText[] TextArray)
        {
            foreach (var fragmentText in TextArray)
            {
                foreach (var symbol in fragmentText.Text)
                {
                    if (!working)
                        return;

                    mutex?.WaitOne();

                    dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() =>
                    {
                        if (element.Text.Length > 0 && element.Text[element.Text.Length - 1].ToString() == ConfigManager.Config.SpecialSymbol)
                            element.Text = element.Text.Insert(element.Text.Length - 1, symbol.ToString());
                        else
                            element.Text += symbol.ToString();
                    }));

                    scroller.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        scroller.ScrollToBottom();
                    }));

                    mutex?.ReleaseMutex();

                    if (fragmentText.Delay > 0)
                        Thread.Sleep((int)fragmentText.Delay);
                }
            }
        }

        // Method to print list string to textblock with delay
        public static void PrintLines(TextBox element, ScrollViewer scroller, Dispatcher dispatcher, ref bool working, Mutex mutex = default, params FragmentText[] TextArray)
        {
            foreach (var fragmentText in TextArray)
            {
                foreach (var symbol in fragmentText.Text)
                {
                    if (!working)
                        return;

                    mutex?.WaitOne();

                    dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() =>
                    {
                        if (element.Text.Length > 0 && element.Text[element.Text.Length - 1].ToString() == ConfigManager.Config.SpecialSymbol)
                            element.Text = element.Text.Insert(element.Text.Length - 1, symbol.ToString());
                        else
                            element.Text += symbol.ToString();
                    }));
                    scroller.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        scroller.ScrollToBottom();
                    }));

                    mutex?.ReleaseMutex();

                    if (fragmentText.Delay > 0)
                        Thread.Sleep((int)fragmentText.Delay);
                }
            }
            dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() =>
                    {
                        element.Focus();
                    }));
        }

        // Get page file by filename
        public static Page GetPageByFilename(string filename, string theme, bool clearPage = false)
        {
            var exct = Path.GetExtension(filename).Remove(0, 1);

            if (Audio.Contains(exct))
                return new AudioViewPage(filename, theme, clearPage);

            if (Video.Contains(exct))
                return new VideoViewPage(filename, theme, clearPage);

            if (Image.Contains(exct))
                return new PictureViewPage(filename, theme, clearPage);

            if (Text.Contains(exct))
                return new TextViewPage(filename, theme, clearPage);

            if (Command.Contains(exct))
                return new TextViewPage(filename, theme, clearPage, isItCommand: true);




            return new TextViewPage(filename, theme, clearPage);
        }
        public static bool IsItPage(string filename)
        {
            var exct = Path.GetExtension(filename).Remove(0, 1);
            if (Audio.Contains(exct) || Video.Contains(exct) || Image.Contains(exct) || Text.Contains(exct) || Command.Contains(exct))
            {
                return true;
            }
            return false;
        }
        public static Window GetWindowByFilename(string filename, string theme)
        {
            var exct = Path.GetExtension(filename).Remove(0, 1);
            var appName = Path.GetFileName(filename).Split('.')[0];
            if (Execute.Contains(exct))
            {
                if (appName== "labirint")
                    return new LabirintGameWindow(theme);
                if (appName == "sputnik")
                    return new SputnikGameWindow(theme);
            }
            return null;
        }
    }
}