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
        int r = 10; // radius of cursor's pickup

        PathGeometry pathGeometry;
        PathFigure pathFigure;
        PolyLineSegment bezie;
        Station start, finish;
        void LoadBase()
        {
            StreamReader reader = new StreamReader(File.OpenRead("Stations.txt"));
            while (!reader.EndOfStream)
            {
                Station station=new Station(reader.ReadLine());
                if(station.Line.Contains('_'))
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
            int x =(int)p.X;
            int y =(int)p.Y;
            Station station = null;
            foreach (List<Station> list in map.Values)
            {
                station = list.FirstOrDefault(st => (st.coord.X - x) * (st.coord.X - x) + (st.coord.Y - y) * (st.coord.Y - y) < r * r); //Coords of mouse are in r from any station
                if (station != null) return station;
            }
            return station;
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadBase();
        }
        
        private void MoveByLine(List<Station> currentLine, int GoFrom, int GoTo, PolyLineSegment bezie)
        {
            
            int sign = Math.Sign(GoTo - GoFrom);
            for (int i = GoFrom;
                sign * i < sign * GoTo;
                i += sign)
            {
                bezie.Points.Add(currentLine[i].coord);
            }
        }

        private void DrawPass()
        {

            pathGeometry = new PathGeometry();
            pathFigure = new PathFigure();
            pathFigure.StartPoint = start.coord;
            bezie = new PolyLineSegment();
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


            pathFigure.Segments.Add(bezie);
            pathGeometry.Figures.Add(pathFigure);
        }

        private void DoAnimation()
        {

            NameScope.SetNameScope(this, new NameScope());

            Rectangle aRectangle = new Rectangle();
            aRectangle.Width = 40;
            aRectangle.Height = 40;
            var br = new ImageBrush();
            br.ImageSource = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "ball.png")));
            br.Stretch = Stretch.UniformToFill;
            aRectangle.Fill = br;

            TranslateTransform transform = new TranslateTransform();
            this.RegisterName("AnimatedTranslateTransform", transform);

            aRectangle.RenderTransform = transform;
            mainPanel.Children.Add(aRectangle);
            this.Content = mainPanel;

            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure();
            pFigure.StartPoint = start.coord;
            PolyLineSegment pLineSegment = bezie;
            pFigure.Segments.Add(pLineSegment);
            animationPath.Figures.Add(pFigure);

            DoubleAnimationUsingPath transX = new DoubleAnimationUsingPath();
            transX.PathGeometry = animationPath;
            transX.Duration = TimeSpan.FromSeconds(5);
            transX.Source = PathAnimationSource.X;
            Storyboard.SetTargetName(transX, "AnimatedTranslateTransform");
            Storyboard.SetTargetProperty(transX,  new PropertyPath(TranslateTransform.XProperty));

            DoubleAnimationUsingPath transY = new DoubleAnimationUsingPath();
            transY.PathGeometry = animationPath;
            transY.Duration = TimeSpan.FromSeconds(5);
            transY.Source = PathAnimationSource.Y;
            Storyboard.SetTargetName(transY, "AnimatedTranslateTransform");
            Storyboard.SetTargetProperty(transY,  new PropertyPath(TranslateTransform.YProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(transX);
            storyboard.Children.Add(transY);
            aRectangle.Loaded += delegate(object sender, RoutedEventArgs e)
            {
                storyboard.Begin(this);
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
            StreamWriter writer= new StreamWriter(File.OpenWrite("Stations.txt"));
            foreach (var list in map.Values)
                foreach (var station in list)
                    writer.WriteLine(station.ToString());
            foreach (var station in passStations)
                writer.WriteLine(station.ToString());
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
