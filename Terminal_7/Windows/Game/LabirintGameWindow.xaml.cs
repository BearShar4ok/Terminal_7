using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using Terminal_7.Classes;

namespace Terminal_7.Windows.Game
{
    /// <summary>
    /// Логика взаимодействия для LabirintGameWindow.xaml
    /// </summary>
    /// 
    public class BoundingBox
    {
        public int posX = 0, posY = 0;
        public int width, height;

        public BoundingBox(int posX, int posY, int width, int height)
        {
            this.posX = posX;
            this.posY = posY;
            this.width = width;
            this.height = height;
        }
    }
    public class SaveData
    {
        public Dictionary<string, List<BoundingBox>> Collisions { get; set; } = new Dictionary<string, List<BoundingBox>>();
        public Dictionary<string, List<(Point pos, double angle)>> Elements { get; set; } = new Dictionary<string, List<(Point pos, double angle)>>();
    }
    public partial class LabirintGameWindow : Window
    {
        int score = 0;
        int maxScore = 0;
        const int blockSize = 32;
        List<Rect> boundsWalls = new List<Rect>();
        List<Rect> boundsCoal = new List<Rect>();
        Rectangle player = new Rectangle();
        Rect playerRect = new Rect();
        int speed;
        string theme;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        SaveData saveData;
        BoundingBox cave = new BoundingBox(0, 0, 0, 0);
        Random random = new Random();



        const string mapName = "map.json";
        ImageBrush groungImageBrush = new ImageBrush();
        ImageBrush floorImageBrush = new ImageBrush();
        string mapText;

        Dictionary<string, ImageBrush> textures = new Dictionary<string, ImageBrush>();



        public LabirintGameWindow(string theme)
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
            Focus();
            this.theme = theme;
            if (!LoadTheme())
            {
                Loaded += (sender, e) =>
                {
                    Close();
                };
                return;
            }
            LoadParams();
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            speed = 4;

            dispatcherTimer.Tag = 0;
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(10);




            maxScore = saveData?.Collisions["Item"].Count ?? 0;
            field.Width = Width;
            field.Height = Height;
            field.Focus();





            Loaded += (sender, e) =>
            {
                DrawMap();
                dispatcherTimer.Start();
            };

            KeyDown += KeyPress;


        }
        private bool LoadTheme()
        {
            var nameFont = "Font.ttf";
            var fullpath = System.IO.Path.GetFullPath(Addition.Themes + theme + "/" + nameFont);

            var families = Fonts.GetFontFamilies(fullpath);
            var family1 = families.First();

            infoText.FontFamily = family1;


            try
            {
                string path = System.IO.Path.GetFullPath(Addition.Themes + theme + "/" + Addition.LabirintGame + "/" + mapName);
                mapText = File.ReadAllText(path);
                groungImageBrush.ImageSource = new BitmapImage(new Uri(Addition.Themes + theme + "/" + Addition.LabirintGame + "/backunder.png", UriKind.Relative));
                
                floorImageBrush = new ImageBrush(new BitmapImage(new Uri(Addition.Themes + theme + "/" + Addition.LabirintGame  + "/floor.png", UriKind.Relative)));

                field.Background = new ImageBrush(new BitmapImage(new Uri(Addition.Themes + theme + "/" + Addition.LabirintGame + "/backsky.png", UriKind.Relative)));

                saveData = LoadMap();
                if (saveData == null)
                    throw new Exception("saveData == null");
                foreach (var textureName in saveData.Elements.Keys)
                {
                    if (textureName == "ToTest.png" || textureName == "Spawn.png")
                        continue;
                    textures.Add(textureName, new ImageBrush(new BitmapImage(new Uri(Addition.Themes + theme + "/" + Addition.LabirintGame + "/" + textureName, UriKind.Relative))));
                }
                player.Fill = new ImageBrush(new BitmapImage(new Uri(Addition.Themes + theme + "/" + Addition.LabirintGame + "/player.png", UriKind.Relative)));
            }
            catch (Exception)
            {
                dispatcherTimer.Stop();
                AlertWindow aw = new AlertWindow("Ошибка!!!", "Файлы для запуска шахты повреждены.", "Закрыть", theme);
                if (aw.ShowDialog() == false)
                {
                    //Close();
                }
                return false;
            }
            return true;
        }
        private void LoadParams()
        {
            infoText.Text = $"Собрано драгоценных камней: {0} из {0}";
            infoText.FontSize = ConfigManager.Config.FontSize;
            infoText.Foreground = (Brush)new BrushConverter().ConvertFromString(ConfigManager.Config.TerminalColor);
           // field.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F94A7"));

        }
        private SaveData LoadMap()
        {
            var data = JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(mapText);
            var result = new SaveData();

            foreach (var pair in data[0])
            {
                var lst = new List<BoundingBox>();

                foreach (JObject obj in pair.Value) lst.Add(new BoundingBox(
                    obj["posX"]?.Value<int>() ?? 0,
                    obj["posY"]?.Value<int>() ?? 0,
                    obj["width"]?.Value<int>() ?? 0,
                    obj["height"]?.Value<int>() ?? 0
                ));

                result.Collisions.Add(pair.Key, lst);
            }

            foreach (var pair in data[1])
            {
                var lst = new List<(Point pos, double angle)>();

                foreach (JObject obj in pair.Value)
                {
                    var strPoint = obj["Item1"]?.Value<string>() ?? "0,0";

                    lst.Add((
                        new Point(int.Parse(strPoint.Split(',')[0]), int.Parse(strPoint.Split(',')[1])),
                        obj["Item2"]?.Value<double>() ?? 0
                    ));
                }

                result.Elements.Add(pair.Key, lst);
            }


            var playerPos = result.Elements["Spawn.png"][0].pos;



            foreach (var item in result.Collisions["Item"])
            {
                if (item.posX == playerPos.X && item.posY == playerPos.Y)
                {
                    result.Collisions["Item"].Remove(item);
                    break;
                }
            }

            int maxX = 0;
            int maxY = 0;
            foreach (var item in result.Collisions["Wall"])
            {
                if (item.posX + item.width > maxX)
                {
                    maxX = item.posX + item.width;
                }
                if (item.posY + item.height > maxY)
                {
                    maxY = item.posY + item.height;
                }
            }

            cave.posX = 0;
            cave.posY = 0;
            cave.width = maxX;
            cave.height = maxY;



            return result;





        }
        private void DrawMap()
        {

            int sdvigX = (int)(field.ActualWidth - cave.width) / 2;
            int sdvigY = (int)(field.ActualHeight - cave.height) / 2;
            player.Width = 32;
            player.Height = 32;




            Canvas.SetTop(player, saveData.Elements["Spawn.png"][0].pos.Y + sdvigY + 8);
            Canvas.SetLeft(player, saveData.Elements["Spawn.png"][0].pos.X + sdvigX + 8);



            Rectangle caveZone = new Rectangle();
            caveZone.Width = cave.width;
            caveZone.Height = cave.height;
            Canvas.SetTop(caveZone, cave.posY + sdvigY);
            Canvas.SetLeft(caveZone, cave.posX + sdvigX);
            caveZone.Fill = new SolidColorBrush(System.Windows.Media.Colors.Green);
            caveZone.Opacity = 0.7;



            Rectangle underground = new Rectangle();
            underground.Width = field.ActualWidth;
            underground.Height = field.ActualHeight;

            groungImageBrush.Stretch = Stretch.None; // Устанавливаем Stretch в None
            groungImageBrush.AlignmentY = AlignmentY.Top;// Устанавливаем Stretch в None
            underground.Fill = groungImageBrush;

            //leftWall.Opacity = 0.3;
            Canvas.SetTop(underground, cave.posY + sdvigY);
            Canvas.SetLeft(underground, 0);
            field.Children.Add(underground);



            Rectangle leftWall = new Rectangle();
            leftWall.Width = (field.ActualWidth - cave.width) / 2;
            leftWall.Height = field.ActualHeight - sdvigY;
            leftWall.Fill = new SolidColorBrush(System.Windows.Media.Colors.Black);
            //leftWall.Opacity = 0.3;
            Canvas.SetTop(leftWall, cave.posY + sdvigY);
            Canvas.SetLeft(leftWall, 0);


            Rectangle rightWall = new Rectangle();
            rightWall.Width = (field.ActualWidth - cave.width) / 2;
            rightWall.Height = field.ActualHeight - sdvigY;
            rightWall.Fill = new SolidColorBrush(System.Windows.Media.Colors.Black);
            //rightWall.Opacity = 0.3;
            Canvas.SetTop(rightWall, cave.posY + sdvigY);
            Canvas.SetLeft(rightWall, cave.posX + cave.width + sdvigX);


            Rectangle bottomWall = new Rectangle();
            bottomWall.Width = cave.width;
            bottomWall.Height = field.ActualHeight - sdvigY - cave.height;
            bottomWall.Fill = new SolidColorBrush(System.Windows.Media.Colors.Black);
            //bottomWall.Opacity = 0.3;
            Canvas.SetTop(bottomWall, cave.posY + cave.height + sdvigY);
            Canvas.SetLeft(bottomWall, cave.posX + sdvigX);


            for (int i = 0; i < caveZone.Width / blockSize; i++)
            {
                for (int j = 0; j < caveZone.Height / blockSize; j++)
                {
                    Rectangle r = new Rectangle();
                    r.Width = blockSize;
                    r.Height = blockSize;
                    r.Fill = floorImageBrush;

                    field.Children.Add(r);
                    Canvas.SetTop(r, cave.posY + sdvigY + j * 32);
                    Canvas.SetLeft(r, cave.posX + sdvigX + i * 32);
                }
            }

            for (int i = 0; i < saveData.Collisions["Wall"].Count; i++)
            {
                BoundingBox position = saveData.Collisions["Wall"][i];
                position.posX += sdvigX;
                position.posY += sdvigY;


                int blocksInWidth = position.width / blockSize;
                int blocksInHeight = position.height / blockSize;

                for (int y = 0; y < blocksInHeight; y++)
                {
                    for (int x = 0; x < blocksInWidth; x++)
                    {
                        Rect rect = new Rect();
                        rect.Width = blockSize;
                        rect.Height = blockSize;
                        rect.X = position.posX + x * blockSize;
                        rect.Y = position.posY + y * blockSize;
                        boundsWalls.Add(rect);

                        Rectangle r = new Rectangle();
                        r.Width = blockSize;
                        r.Height = blockSize;
                        string textureName = FindTextureName((int)rect.X - sdvigX, (int)rect.Y - sdvigY);
                        //r.Fill = textures[textureName];


                        try
                        {
                            r.Fill = new ImageBrush(new BitmapImage(new Uri(Addition.Themes + theme+"/" + Addition.LabirintGame + "/" + textureName, UriKind.Relative)));
                           
                        }
                        catch (Exception)
                        {
                            dispatcherTimer.Stop();
                            AlertWindow aw = new AlertWindow("Ошибка!!!", "У шахты повреждены стены.", "Закрыть", theme);
                            if (aw.ShowDialog() == false)
                            {
                               // Close();
                            }
                            return;
                        }


                        field.Children.Add(r);
                        Canvas.SetTop(r, rect.Y);
                        Canvas.SetLeft(r, rect.X);
                    }
                }
            }


            for (int i = 0; i < saveData.Collisions["Item"].Count; i++)
            {
                BoundingBox position = saveData.Collisions["Item"][i];
                position.posX += sdvigX;
                position.posY += sdvigY;

                Rect rect = new Rect();
                rect.Width = position.width;
                rect.Height = position.height;
                rect.X = position.posX;
                rect.Y = position.posY;
                boundsCoal.Add(rect);

                Rectangle r = new Rectangle();
                r.Width = position.width;
                r.Height = position.height;

                try
                {
                    r.Fill = new ImageBrush(new BitmapImage(new Uri(Addition.Themes + theme + "/" + Addition.LabirintGame + "/" + $"coal{random.Next(1, 4)}.png", UriKind.Relative)));
                }
                catch (Exception)
                {
                    dispatcherTimer.Stop();
                    AlertWindow aw = new AlertWindow("Ошибка!!!", "У шахты повреждены стены.", "Закрыть", theme);
                    if (aw.ShowDialog() == false)
                    {
                        //Close();
                    }
                    return;
                }
                r.Tag = "Coal";
                field.Children.Add(r);
                Canvas.SetTop(r, position.posY);
                Canvas.SetLeft(r, position.posX);
            }


            ////field.Children.Add(caveZone);
            //field.Children.Add(leftWall);
            //field.Children.Add(rightWall);
            //field.Children.Add(bottomWall);

            field.Children.Add(player);

        }
        private string FindTextureName(int x, int y)
        {

            foreach (var key in saveData.Elements.Keys)
            {
                foreach (var item in saveData.Elements[key])
                {
                    if (item.pos.X == x && item.pos.Y == y)
                    {
                        return key;
                    }
                }
            }
            return "default.png";
        }
        private bool IsIntersectCoal()
        {
            playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            foreach (Rect r in boundsCoal)
            {
                if (r.IntersectsWith(playerRect))
                {
                    Rect r2 = r;
                    r2.Intersect(playerRect);
                    if (r2.Width > player.Width / 2 && r2.Height > player.Height / 2)
                    {
                        boundsCoal.Remove(r);
                        field.Children.Remove(FindRectangle(r));
                        score++;
                        return true;
                    }
                    return false;

                }
            }
            return false;
        }
        private Rectangle FindRectangle(Rect rect)
        {
            foreach (var re in field.Children)
            {
                if (re is Rectangle)
                {
                    Rectangle r = (Rectangle)re;
                    if (r.Width == rect.Width && r.Height == rect.Height && Canvas.GetLeft(r) == rect.X && Canvas.GetTop(r) == rect.Y && r.Tag != null && r.Tag == "Coal")
                    {
                        return r;
                    }
                }
            }
            return null;
        }
        private bool IsIntersects()
        {
            playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            foreach (Rect r in boundsWalls)
            {

                if (r.IntersectsWith(playerRect))
                {
                    Rect r2 = r;
                    r2.Intersect(playerRect);
                    if (r2.Width > 0 && r2.Height > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            if (IsIntersects())
            {
                dispatcherTimer.Stop();
                AlertWindow aw = new AlertWindow("Уведомление", "Поломка!!! Вы повредили устройство.", "Закрыть", theme);
                if (aw.ShowDialog() == false)
                {
                    Close();
                }
            }
            if (IsIntersectCoal())
            {
                if (score >= maxScore)
                {
                    dispatcherTimer.Stop();
                    AlertWindow aw = new AlertWindow("Уведомление", "Все драгоценные камни собраны.", "Закрыть", theme);
                    if (aw.ShowDialog() == false)
                    {
                        Close();
                    }
                }
            }
            infoText.Text = $"Собрано драгоценных камней: {score} из {maxScore}";
            // }));

        }
        private void KeyPress(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.W:
                    Canvas.SetTop(player, Canvas.GetTop(player) - speed);
                    playerRect.Y -= speed;
                    break;
                case Key.A:
                    Canvas.SetLeft(player, Canvas.GetLeft(player) - speed);
                    playerRect.Y -= speed;
                    break;
                case Key.S:
                    Canvas.SetTop(player, Canvas.GetTop(player) + speed);
                    playerRect.Y += speed;
                    break;
                case Key.D:
                    Canvas.SetLeft(player, Canvas.GetLeft(player) + speed);
                    playerRect.Y += speed;
                    break;
                default:
                    break;
            }
        }
    }
}
