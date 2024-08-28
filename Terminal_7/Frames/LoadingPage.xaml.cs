using System;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using Terminal_7.Classes;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;
using Terminal_7.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Terminal_7.Frames
{
    public enum IconTypeExplorer { Default, Folder, Image, Text, Audio, Video, Command, Execute }
    public enum IconTypeNotification { Copy, Hack }

    //  🖹🖻🖺

    public partial class LoadingPage : Page
    {
        private const string AccessFileToReadDisk = "READ_DISK.txt";
        private const string AccessFileToHack = "HACK.txt";
        private const string PrevDirText = "..";
        private const string SystemFolder = "System Volume Information";
        private const string ExtensionConfig = ".config";
        private const string _NetDiskovText = "Доступных дисков нет...";
        private const string TrashbinPath = "\\TrashBin";
        private readonly Dictionary<IconTypeExplorer, string> IconsExplorer;
        private readonly Dictionary<IconTypeNotification, string> IconsNotification;

        private string _theme;
        private int _deepOfPath;
        private int _selectedIndex;
        private KeyStates _prevkeyState;
        private string _currDisk;


        private Dictionary<string, ListBoxItem> _disks = new Dictionary<string, ListBoxItem>();
        private List<string> _disksToHack = new List<string>();
        private Dictionary<string, int> _huckAttempts = new Dictionary<string, int>();

        public static RoutedCommand CopyCommand = new RoutedCommand();
        public static RoutedCommand PasteCommand = new RoutedCommand();
        public static RoutedCommand DeleteCommand = new RoutedCommand();
        private string copyPath;
        private string disksPath;

        private bool hackAllow;

        public LoadingPage(string theme)
        {
            InitializeComponent();

            // Add actions to devices
            DevicesManager.AddDisk += disk => AddDisk(disk);
            DevicesManager.RemoveDisk += RemoveDisk;

            _theme = theme;

            KeepAlive = true;

            CopyCommand.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));
            PasteCommand.InputGestures.Add(new KeyGesture(Key.V, ModifierKeys.Control));
            DeleteCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            LblInfo.Content = _NetDiskovText;

            LB.SelectionMode = SelectionMode.Single;
            LB.SelectedIndex = 0;
            LB.FocusVisualStyle = null;
            LB.Focus();

            KeyDown += AdditionalKeys;

            IconsExplorer = new Dictionary<IconTypeExplorer, string>()
            {
                { IconTypeExplorer.Default, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/default.png") },
                { IconTypeExplorer.Folder, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/folder.png") },
                { IconTypeExplorer.Image, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/image.png") },
                { IconTypeExplorer.Text, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/text.png") },
                { IconTypeExplorer.Audio, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/audio.png") },
                { IconTypeExplorer.Command, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/command.png") },
                { IconTypeExplorer.Video, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/video.png") },
                { IconTypeExplorer.Execute, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/execute.png") }
            };

            IconsNotification = new Dictionary<IconTypeNotification, string>()
            {
                 { IconTypeNotification.Copy, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/copy.png") },
                 { IconTypeNotification.Hack, Path.GetFullPath(Addition.Themes + theme +  $@"/{Addition.Icons}/hack.png") }
            };

            LoadParams();
            LoadTheme();
            DevicesManager.StartListening();
        }
        private void LoadTheme()
        {
            var nameFont = "Font.ttf";
            var fullpath = Path.GetFullPath(Addition.Themes + _theme + "/" + nameFont);

            var families = Fonts.GetFontFamilies(fullpath);
            var family1 = families.First();
            LblInfo.FontFamily = family1;
        }

        private void LoadParams()
        {
            LblInfo.FontSize = ConfigManager.Config.FontSize;
            LblInfo.Opacity = ConfigManager.Config.Opacity;
            LblInfo.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);

            HackIco.Source = new BitmapImage(new Uri(IconsNotification[IconTypeNotification.Hack]));
            HackIco.Visibility = Visibility.Hidden;

            CopyIco.Source = new BitmapImage(new Uri(IconsNotification[IconTypeNotification.Copy]));
            CopyIco.Visibility = Visibility.Hidden;

            hackAllow = false;
        }
        private void AddDisk(string disk, bool addToList = true)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                try
                {
                    if (!addToList)
                    {
                        LB.Items.Add(_disks[disk]);
                        return;
                    }

                    var allFiles = Directory.GetFiles(disk).Select(Path.GetFileName).ToArray();

                    if (ConfigManager.Config.IsFlashcardHack)
                    {
                        if (allFiles.Contains(AccessFileToHack))
                        {
                            _disksToHack.Add(disk);
                            HackIco.Visibility = Visibility.Visible;
                            hackAllow = true;
                        }
                    }
                    
                        

                    if (!allFiles.Contains(AccessFileToReadDisk)) 
                        return;


                    var fullPath = File.ReadAllText(disk + AccessFileToReadDisk);
                    disksPath = Path.GetDirectoryName(fullPath);
                    if (Directory.Exists(fullPath))
                    {
                        LblInfo.Content = "";
                        LblInfo.Visibility = Visibility.Hidden;
                        var diskName = Path.GetFileNameWithoutExtension(fullPath);
                        //var diskName = Path.GetFileNameWithoutExtension(fullPath);

                        var lbi = new ListBoxItem()
                        {
                            DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Folder])),
                            Content = diskName,
                            Tag = fullPath,
                            Style = (Style)App.Current.FindResource("ImageText"),
                            Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                            FontFamily = LblInfo.FontFamily,
                            FontSize = LblInfo.FontSize,
                        };

                        if (_deepOfPath == 0)
                            LB.Items.Add(lbi);

                        _disks.Add(disk, lbi);
                    }
                    Focus();
                }
                catch { }
            }));
        }

        private void RemoveDisk(string diskName)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                var disk = _disks.Pop(diskName);

                if (LB.Items.Contains(disk))
                    LB.Items.Remove(disk);

                if (_currDisk == diskName)
                {
                    LB.SelectedIndex = 0;
                    _selectedIndex = 0;
                    _currDisk = null;
                    LB.Items.Clear();
                    _disks.Keys.ForEach(x => AddDisk(x, false));
                }
                if (LB.Items.Count == 0)
                {
                    LblInfo.Content = _NetDiskovText;
                    LblInfo.Visibility = Visibility.Visible;
                }
                if (_disksToHack.Contains(diskName))
                {
                    HackIco.Visibility = Visibility.Hidden;
                    _disksToHack.Remove(diskName);
                    hackAllow = false;
                }
                    
            }));
        }

        private void lstB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lbi = (ListBoxItem)LB.SelectedItem;
            if (lbi == null)
                return;

            var directory = lbi.Tag.ToString();

            if (_currDisk == null)
                _currDisk = _disks.FindKey(lbi);

            if (directory.EndsWith(PrevDirText))
            {
                if (_deepOfPath == 0) return;
                _deepOfPath--;

                if (_deepOfPath > 0)
                {
                    Open(directory.RemoveLast("\\").RemoveLast("\\"), true, true);
                    return;
                }

                LB.SelectedIndex = 0;
                _selectedIndex = 0;
                _currDisk = null;
                LB.Items.Clear();
                _disks.Keys.ForEach(x => AddDisk(x, false));
                return;
            }

            if (IsFolder(directory))
            {
                _deepOfPath++;
                Open(directory, true);
            }
            else
            {
                Open(directory, false);
            }
        }

        // Look for all files in directory
        private void FindFiles(string directory)
        {
            var files = Directory.GetFiles(directory).ToList();
            var directories = Directory.GetDirectories(directory);

            for (var i = 0; i < files.Count; i++)
            {
                if (files[i].Contains(ExtensionConfig) && (files.Contains(files[i].RemoveLast(ExtensionConfig)) || directories.Contains(files[i].RemoveLast(ExtensionConfig))))
                {
                    files.RemoveAt(i);
                    i--;
                }
            }

            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var name = Path.GetFileNameWithoutExtension(file);

                var extension = Path.GetExtension(file).Remove(0, 1);

                var lbi = new ListBoxItem()
                {
                    Content = name,
                    Tag = $@"{directory}\{filename}",
                    Style = (Style)App.Current.FindResource("ImageText"),
                    Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                    FontFamily = LblInfo.FontFamily,
                    FontSize = LblInfo.FontSize,

                };

                if (Addition.Text.Contains(extension))
                    lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Text]));
                else if (Addition.Image.Contains(extension))
                    lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Image]));
                else if (Addition.Audio.Contains(extension))
                    lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Audio]));
                else if (Addition.Video.Contains(extension))
                    lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Video]));
                else if (Addition.Command.Contains(extension))
                    lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Command]));
                else if (Addition.Execute.Contains(extension))
                    lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Execute]));
                else
                    lbi.DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Default]));

                LB.Items.Add(lbi);
            }
        }

        // Look for all folders in directory
        private void FindFolders(string directory)
        {
            LB.Items.Clear();
            var allDirectories = Directory.GetDirectories(directory);

            if (_deepOfPath > 0)
            {
                var lbi = new ListBoxItem()
                {
                    DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Folder])),
                    Content = PrevDirText,
                    Tag = $@"{directory}\{PrevDirText}",
                    Style = (Style)App.Current.FindResource("ImageText"),
                    Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                    FontFamily = LblInfo.FontFamily,
                    FontSize = LblInfo.FontSize,

                };

                LB.Items.Add(lbi);
            }

            foreach (var dir in allDirectories)
            {
                var name = Path.GetFileName(dir);
                if (name == SystemFolder) continue;

                var lbi = new ListBoxItem()
                {
                    DataContext = new BitmapImage(new Uri(IconsExplorer[IconTypeExplorer.Folder])),
                    Content = name,
                    Tag = $@"{directory}\{name}",
                    Style = (Style)App.Current.FindResource("ImageText"),
                    Foreground = (Brush)new BrushConverter().ConvertFrom(ConfigManager.Config.TerminalColor),
                    FontFamily = LblInfo.FontFamily,
                    FontSize = LblInfo.FontSize,

                };

                LB.Items.Add(lbi);
            }

        }

        // All additional keys, which cant be used in System hotkeys
        private void AdditionalKeys(object sender, KeyEventArgs e)
        {
            if (_prevkeyState == e.KeyStates) return;

            switch (e.Key)
            {
                case Key.Enter:
                    lstB_MouseDoubleClick(null, null);
                    break;
                case Key.Escape:
                    if (ConfigManager.Config.IsDebugMode)
                    {
                        App.Current.MainWindow.Close();
                    }
                    break;
            }

            _prevkeyState = e.KeyStates;
        }

        private void GoToFilePage(string directory)
        {
            if (Addition.IsItPage(directory))
            {
                var nextPage = Addition.GetPageByFilename(directory, _theme);

                if (nextPage != default)
                    Addition.NavigationService.Navigate(nextPage);
            }
            else
            {
                var window = Addition.GetWindowByFilename(directory, _theme);
                window.ShowDialog();
            }


        }

        private void AccessInFolderOpen(string directory)
        {
            FindFolders(directory);
            FindFiles(directory);

            LB.SelectedIndex = 0;
            _selectedIndex = 0;
            LB.Focus();
        }

        private void Open(string directory, bool isFolder, bool isGoBack = false)
        {
            if (Directory.GetFiles(directory.RemoveLast(@"\")).Contains(directory + ".config") && !isGoBack)
            {
                try
                {
                    var content = JsonConvert.DeserializeObject<ConfigDeserializer>(File.ReadAllText(directory + ".config"));

                    if (!content.HasPassword)
                    {
                        if (isFolder)
                            AccessInFolderOpen(directory);
                        else
                            GoToFilePage(directory);
                    }
                    else
                    {
                        var lw = new LoginWindow(_theme, NormalizeLoginANdPassword(content.LoginsAndPasswords));
                        if (lw.ShowDialog() == false)
                        {
                            if (lw.ReternedState == State.Access)
                            {
                                if (isFolder)
                                    AccessInFolderOpen(directory);
                                else
                                    GoToFilePage(directory);
                            }
                            else if (lw.ReternedState == State.Cancel)
                            {
                                _deepOfPath--;
                                return;
                            }
                            else if (lw.ReternedState == State.Hack)
                            {
                                if (content.CanBeHacked)
                                {

                                    if (ConfigManager.Config.IsFlashcardHack && !hackAllow)
                                    {
                                        var alert = new AlertWindow("Уведомление", "Для взлома требуется внешний носитель с соответствующим ПО.", "Закрыть", _theme);
                                        if (alert.ShowDialog() == false)
                                        {
                                            return;
                                        }
                                    }

                                    ///
                                    var hw = new HackWindow(_theme, lw.Password, content.HackAttempts);
                                    //lw.Close();


                                    if (hw.ShowDialog() == false)
                                    {
                                        if (hw.ReternedState == State.Access)
                                        {
                                            if (isFolder)
                                                AccessInFolderOpen(directory);
                                            else
                                                GoToFilePage(directory);
                                        }
                                        //content.CanBeHacked = false;
                                        File.WriteAllText(directory + ".config", JsonConvert.SerializeObject(content));

                                    }

                                }
                                else
                                {
                                    var alert = new AlertWindow("Уведомление", "Недостаточно прав для взлома.", "Закрыть", _theme);
                                    if (alert.ShowDialog() == false)
                                    {

                                    }
                                }
                            }
                        }

                    }
                }
                catch
                {
                    if (isFolder)
                        AccessInFolderOpen(directory);
                    else
                        GoToFilePage(directory);
                }
            }
            else
            {
                if (isFolder)
                    AccessInFolderOpen(directory);
                else
                    GoToFilePage(directory);
            }
        }

        private Dictionary<string, string> NormalizeLoginANdPassword(Dictionary<string, string> dct)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var log in dct)
            {
                result.Add(log.Key.ToLower(), log.Value.ToLower());
            }
            return result;
        }
        private static bool IsFolder(string path) => Directory.Exists(path);
        private void ExecuteDeleteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var lbi = (ListBoxItem)LB.SelectedItem;
            if (lbi == null)
                return;

            copyPath = lbi.Tag.ToString();
            if (File.Exists(copyPath + ".config"))
            {
                var content = JsonConvert.DeserializeObject<ConfigDeserializer>(File.ReadAllText(copyPath + ".config"));
                 
                if (!content.CanBeDeleted)
                {
                    var alert = new AlertWindow("Ошибка", "Недостаточно прав для удаления.", "Закрыть", _theme);
                    if (alert.ShowDialog() == false)
                    {
                        return;
                    }
                }
            }
            string destinationDirectory = disksPath + TrashbinPath;
            string destinationFile = destinationDirectory + "\\" + Path.GetFileName(copyPath);


            try
            {
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);

                }

                if (!File.Exists(destinationFile))
                {
                    File.Move(copyPath, destinationFile);
                    if (File.Exists(copyPath + ExtensionConfig))
                        File.Move(copyPath + ExtensionConfig, destinationFile + ExtensionConfig);
                }
                else
                {
                    File.Copy(copyPath, destinationFile, true);
                    if (File.Exists(copyPath + ExtensionConfig))
                        File.Copy(copyPath + ExtensionConfig, destinationFile + ExtensionConfig, true);
                    File.Delete(copyPath);
                    if (File.Exists(copyPath + ExtensionConfig))
                        File.Delete(copyPath + ExtensionConfig);
                }

                var loading = new ProgressAlertWindowText("Удаление", "Процесс удаления...",7,0,true,_theme);
                if (loading.ShowDialog() == false)
                {

                }

                    var alert = new AlertWindow("Уведомление", "Файл удален.", "Закрыть", _theme);
                if (alert.ShowDialog() == false)
                {
                    Open(Path.GetDirectoryName(lbi.Tag.ToString()), true);
                }
            }
            catch (IOException ioEx)
            {

                var alert = new AlertWindow("Ошибка", "Произошла непредвиденная ошибка.\r\n" + ioEx.Message, "Закрыть", _theme);
                if (alert.ShowDialog() == false)
                {

                }
            }
            catch (UnauthorizedAccessException unAuthEx)
            {

                var alert = new AlertWindow("Ошибка", "Произошла непредвиденная ошибка.\r\n" + unAuthEx.Message, "Закрыть", _theme);
                if (alert.ShowDialog() == false)
                {

                }
            }
        }
        private void ExecuteCopyCommand(object sender, ExecutedRoutedEventArgs e)
        {
            
            var lbi = (ListBoxItem)LB.SelectedItem;
            if (lbi == null)
                return;

            copyPath = lbi.Tag.ToString();

            if (File.Exists(copyPath + ".config"))
            {
                var content = JsonConvert.DeserializeObject<ConfigDeserializer>(File.ReadAllText(copyPath + ".config"));

                if (!content.CanBeDeleted)
                {
                    var alert = new AlertWindow("Ошибка", "Недостаточно прав для копирования.", "Закрыть", _theme);
                    if (alert.ShowDialog() == false)
                    {
                        return;
                    }
                }
            }
            CopyIco.Visibility = Visibility.Visible;
        }
        private void ExecutePasteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (CopyIco.Visibility == Visibility.Hidden)
                return;
            CopyIco.Visibility = Visibility.Hidden;


            var lbi = (ListBoxItem)LB.SelectedItem;
            if (lbi == null)
                return;

            string destinationFile = Path.GetDirectoryName(lbi.Tag.ToString()) + "\\" + Path.GetFileName(copyPath);
            //string destinationDirectory = lbi.Tag.ToString());

            try
            {
                File.Copy(copyPath, destinationFile, true);
                if (File.Exists(copyPath + ExtensionConfig))
                    File.Copy(copyPath + ExtensionConfig, destinationFile + ExtensionConfig, true);
                var alert = new AlertWindow("Уведомление", "Файл скопирован.", "Закрыть", _theme);
                if (alert.ShowDialog() == false)
                {
                    Open(Path.GetDirectoryName(lbi.Tag.ToString()), true);
                }    
            }
            catch (IOException ioEx)
            {

                var alert = new AlertWindow("Ошибка", "Произошла непредвиденная ошибка.\r\n" + ioEx.Message, "Закрыть", _theme);
                if (alert.ShowDialog() == false)
                {

                }
            }
            catch (UnauthorizedAccessException unAuthEx)
            {

                var alert = new AlertWindow("Ошибка", "Произошла непредвиденная ошибка.\r\n" + unAuthEx.Message, "Закрыть", _theme);
                if (alert.ShowDialog() == false)
                {

                }
            }
        }
    }
}
