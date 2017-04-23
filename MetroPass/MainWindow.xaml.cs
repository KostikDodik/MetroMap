using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetroPass
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, List<Station>> map = new Dictionary<string, List<Station>>();
        List<Station> passStations = new List<Station>();
        ImageBrush ballBrush = new ImageBrush();
        
        int r = 10; // radius of cursor's pickup

        PolyLineSegment bezie;

        Station start, finish;

        public MainWindow()
        {
            InitializeComponent();
            LoadBase();
            ballBrush.ImageSource = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "ball.png")));
            ballBrush.Stretch = Stretch.UniformToFill;
        }

        void LoadBase()
        {
            StreamReader reader = new StreamReader(File.OpenRead("Stations.txt"));
            while (!reader.EndOfStream)
            {
                Station station = new Station(reader.ReadLine());
                if (station.Line.Contains('_'))
                    passStations.Add(station);
                else
                {
                    if (!map.ContainsKey(station.Line))
                        map.Add(station.Line, new List<Station>());
                    map[station.Line].Add(station);
                }
            }
            reader.Close();
        }

        Station FindStation(Point p)
        {
            int x = (int)p.X;
            int y = (int)p.Y;
            Station station = null;
            foreach (List<Station> list in map.Values)
            {
                station = list.FirstOrDefault(st => (st.coord.X - x) * (st.coord.X - x) + (st.coord.Y - y) * (st.coord.Y - y) < r * r); //Coords of mouse are in r from any station
                if (station != null) return station;
            }
            return station;
        }

        private void AddPoint(PolyLineSegment bezie, Point po)
        {
            Point p = po;
            p.X -= 20;
            p.Y -= 20;
            bezie.Points.Add(p);
        }

        private void MoveByLine(List<Station> currentLine, int GoFrom, int GoTo, PolyLineSegment bezie)
        {

            int sign = Math.Sign(GoTo - GoFrom);
            for (int i = GoFrom;
                sign * i <= sign * GoTo;
                i += sign)
            {
                AddPoint(bezie, currentLine[i].coord);
            }
        }

        private void DrawPass()
        {
            bezie = new PolyLineSegment();
            if (start == finish) return;
            List<Station> currentLine = map[start.Line];

            int GoFrom, GoTo;
            if (start.Line == finish.Line)
            {
                GoFrom = currentLine.IndexOf(start);
                GoTo = currentLine.IndexOf(finish);
                MoveByLine(currentLine, GoFrom, GoTo, bezie);
            }
            else
            {
                string passLine = start.Line + '_' + finish.Line;
                string point = passStations.Find(s => s.Line == passLine).Name; //Name of passage station in starting Line
                GoFrom = currentLine.IndexOf(start);
                GoTo = currentLine.FindIndex(s => s.Name == point);
                MoveByLine(currentLine, GoFrom, GoTo, bezie);



                currentLine = map[finish.Line];
                passLine = finish.Line + '_' + start.Line;
                point = passStations.Find(s => s.Line == passLine).Name;
                GoFrom = currentLine.FindIndex(s => s.Name == point);
                GoTo = currentLine.IndexOf(finish);
                MoveByLine(currentLine, GoFrom, GoTo, bezie);
            }
        }

        private void DoAnimation()
        {
            if (start == finish) return;
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            Point startingFrom = start.coord;
            startingFrom.X -= 20;
            startingFrom.Y -= 20;
            pathFigure.StartPoint = startingFrom;
            pathFigure.Segments.Add(bezie);
            pathGeometry.Figures.Add(pathFigure);

            NameScope.SetNameScope(this, new NameScope());
            DoubleAnimationUsingPath transX = new DoubleAnimationUsingPath();
            DoubleAnimationUsingPath transY = new DoubleAnimationUsingPath();
            Rectangle aRectangle = new Rectangle();
            aRectangle.Width = 40;
            aRectangle.Height = 40;
            aRectangle.Fill = ballBrush;

            TranslateTransform transform = new TranslateTransform();
            this.RegisterName("Transform", transform);

            aRectangle.RenderTransform = transform;
            mainPanel.Children.Add(aRectangle);
            this.Content = mainPanel;

            transX.PathGeometry = pathGeometry;
            transX.Duration = TimeSpan.FromSeconds(5);
            transX.Source = PathAnimationSource.X;
            Storyboard.SetTargetName(transX, "Transform");
            Storyboard.SetTargetProperty(transX, new PropertyPath(TranslateTransform.XProperty));

            transY.PathGeometry = pathGeometry;
            transY.Duration = TimeSpan.FromSeconds(5);
            transY.Source = PathAnimationSource.Y;
            Storyboard.SetTargetName(transY, "Transform");
            Storyboard.SetTargetProperty(transY, new PropertyPath(TranslateTransform.YProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(transX);
            storyboard.Children.Add(transY);
            storyboard.Duration = TimeSpan.FromSeconds(7);


            aRectangle.Loaded += delegate(object sender, RoutedEventArgs e)
            {
                storyboard.Begin(this);
            };

            storyboard.Completed += delegate(object sender, EventArgs e)
            {
                pathGeometry.Clear();
                mainPanel.Children.Remove(aRectangle);
            };
        }
        
        private void Window_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            if ((bool)EditorsChB.IsChecked) return;

            Point p = Mouse.GetPosition(mainPanel);
            Station selected = FindStation(p);
            if (selected != null)
            {
                StationFrom.Text = selected.Name;
                start = selected;
                if (finish != null)
                {
                    DrawPass();
                    DoAnimation();
                }
            }
        }

        private void Window_MouseRightButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            if ((bool)EditorsChB.IsChecked) return;

            Point p = Mouse.GetPosition(mainPanel);
            Station selected = FindStation(p);
            if (selected != null)
            {
                StationWhere.Text = selected.Name;
                finish = selected;
                if (start != null)
                {
                    DrawPass();
                    DoAnimation();
                }
            }
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StreamWriter writer = new StreamWriter(File.OpenWrite("Stations.txt"));
            foreach (var list in map.Values)
                foreach (var station in list)
                {
                    writer.WriteLine(station.ToString());
                }
            foreach (var station in passStations)
            {
                writer.WriteLine(station.ToString());
            }
            writer.Close();
        }

        private void Window_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {

            Point p = Mouse.GetPosition(mainPanel);
            if ((bool)EditorsChB.IsChecked)
            {
                Station station = new Station(StationWhere.Text, StationFrom.Text, p);
                if (station.Line.Contains('_'))
                    passStations.Add(station);
                else
                {
                    if (!map.ContainsKey(station.Line))
                        map.Add(station.Line, new List<Station>());
                    map[station.Line].Add(station);
                }

            }
        }
    }
}
