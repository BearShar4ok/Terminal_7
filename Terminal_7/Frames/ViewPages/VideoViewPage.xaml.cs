﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Terminal_7.Classes;

namespace Terminal_7.Frames.ViewPages
{
    public partial class VideoViewPage : Page
    {
        private string _filename;
        private string _theme;
        private bool _stop;

        public double Volume
        {
            get => MediaPlayer.Volume;
            set
            {
                MediaPlayer.Volume = Math.Max(Math.Min(value, 1.0), 0.0);
            }
        }

        public VideoViewPage(string filename, string theme, bool clearPage = false)
        {
            InitializeComponent();

            if (clearPage)
                Addition.NavigationService.Navigated += RemoveLast;

            _filename = filename;
            _theme = theme;


            MediaPlayer.MediaEnded += (obj, e) => {
                MediaPlayer.Position = new TimeSpan(0, 0, 0);
                MediaPlayer.Play();
            };

            Application.Current.MainWindow.KeyDown += AdditionalKeys;

            LoadTheme(theme);
            LoadVideo();
        }

        private void RemoveLast(object obj, NavigationEventArgs e)
        {
            Addition.NavigationService?.RemoveBackEntry();
        }

        public void Closing()
        {
            Stop();
            Addition.NavigationService.Navigated -= RemoveLast;
            Application.Current.MainWindow.KeyDown -= AdditionalKeys;
        }

        public void Reload()
        {
            LoadTheme(_theme);
            LoadVideo();
        }

        private void LoadVideo()
        {
            if (!File.Exists(_filename))
                return;

            MediaPlayer.Source = new Uri(_filename, UriKind.Relative);

            Stop();
            Play();
        }

        public void Pause() => MediaPlayer.Pause();

        public void Play() => MediaPlayer.Play();

        public void Stop() => MediaPlayer.Stop();

        public void VolumePlus() => Volume += 0.01d;

        public void VolumeMinus() => Volume -= 0.01d;

        private void LoadTheme(string theme)
        {

        }
        private void AdditionalKeys(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Closing();
                    Addition.NavigationService?.GoBack();
                    break;
                case Key.Space:
                    if (_stop)
                        Pause();
                    else
                        Play();
                    _stop = !_stop;
                    break;
                case Key.Up:
                case Key.VolumeUp:
                    VolumePlus();
                    break;
                case Key.Down:
                case Key.VolumeDown:
                    VolumeMinus();
                    break;
            }

        }
    }
}