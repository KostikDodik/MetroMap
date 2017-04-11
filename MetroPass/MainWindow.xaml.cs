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
        List<Station> RedLine =new List<Station>();
        List<Station> BlueLine=new List<Station>();
        List<Station> GreenLine=new List<Station>();
        List<Station> stations = new List<Station>();
        int r = 10; // radius of cursor's pickup

        PathGeometry pathGeometry;
        PathFigure pathFigure;
        PolyLineSegment bezie;
        Station start, finish;
        void LoadBase()
        {
            StreamReader reader = new StreamReader(File.OpenRead("Stations.txt"));
            while (!reader.EndOfStream)
                stations.Add(new Station(reader.ReadLine()));
            reader.Close();
            RedLine = stations.FindAll(st => st.Line == "Red");
            BlueLine = stations.FindAll(st => st.Line == "Blue");
            GreenLine = stations.FindAll(st => st.Line == "Green");
        }
        
        public MainWindow()
        {
            InitializeComponent();
            LoadBase();
        }

        private void Window_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            int x = (int)Mouse.GetPosition(this).X;
            int y = (int)Mouse.GetPosition(this).Y;
            Station selected = stations.FirstOrDefault(st => (st.coord.X - x) * (st.coord.X - x) + (st.coord.Y - y) * (st.coord.Y - y) < r * r); //Coords of mouse are in r from any station
            if (selected != null)
            {

                StationFrom.Text = selected.Name;
                start = selected;
                if (finish != null)
                    DrawPass();
            }
        }
        private void Window_MouseRightButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            int x = (int)Mouse.GetPosition(this).X;
            int y = (int)Mouse.GetPosition(this).Y;
            Station selected = stations.FirstOrDefault(st => (st.coord.X - x) * (st.coord.X - x) + (st.coord.Y - y) * (st.coord.Y - y) < r * r); //Coords of mouse are in r from any station
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

        private void DrawPass()
        {
            int GoTo;
            pathGeometry = new PathGeometry();
            pathFigure=new PathFigure();
            pathFigure.StartPoint = start.coord;
            bezie = new PolyLineSegment();
            if (start.Line == "Red")
            {
                if (finish.Line == "Red")
                {
                    int sign = Math.Sign(RedLine.IndexOf(finish) - RedLine.IndexOf(start));
                    for (int i = RedLine.IndexOf(start); sign * i < sign * RedLine.IndexOf(finish); i += sign)
                        bezie.Points.Add(RedLine[i].coord);
                    bezie.Points.Add(finish.coord);
                }

                else if (finish.Line == "Blue")
                {
                    GoTo = RedLine.FindIndex(st => st.Name == "Хрещатик");
                    int sign = Math.Sign(GoTo - RedLine.IndexOf(start));
                    for (int i = RedLine.IndexOf(start); sign * i < sign * GoTo; i += sign)
                        bezie.Points.Add(RedLine[i].coord);

                    GoTo = BlueLine.FindIndex(st => st.Name == "Майдан Незалежності");
                    bezie.Points.Add(BlueLine[GoTo].coord);

                    sign = Math.Sign(BlueLine.IndexOf(finish) - GoTo);
                    for (int i = GoTo; sign * i < sign * BlueLine.IndexOf(finish); i += sign)
                        bezie.Points.Add(BlueLine[i].coord);
                    bezie.Points.Add(finish.coord);
                }

                else
                {
                    GoTo = RedLine.FindIndex(st => st.Name == "Театральна");
                    int sign = Math.Sign(GoTo - RedLine.IndexOf(start));
                    for (int i = RedLine.IndexOf(start); sign * i < sign * GoTo; i += sign)
                        bezie.Points.Add(RedLine[i].coord);
                    bezie.Points.Add(RedLine[GoTo].coord);

                    GoTo = GreenLine.FindIndex(st => st.Name == "Золоті Ворота");
                    bezie.Points.Add(GreenLine[GoTo].coord);
                    
                    sign = Math.Sign(GreenLine.IndexOf(finish) - GoTo);
                    for (int i = GoTo; sign * i < sign * GreenLine.IndexOf(finish); i += sign)
                        bezie.Points.Add(GreenLine[i].coord);
                    bezie.Points.Add(finish.coord);
                }
            }


            else if (start.Line == "Blue")
            {
                if (finish.Line == "Blue")
                {
                    int sign = Math.Sign(BlueLine.IndexOf(finish) - BlueLine.IndexOf(start));
                    for (int i = BlueLine.IndexOf(start); sign * i < sign * BlueLine.IndexOf(finish); i += sign)
                        bezie.Points.Add(BlueLine[i].coord);
                    bezie.Points.Add(finish.coord);
                }

                else if (finish.Line == "Red")
                {
                    GoTo = BlueLine.FindIndex(st => st.Name == "Майдан Незалежності");
                    int sign = Math.Sign(GoTo - BlueLine.IndexOf(start));
                    for (int i = BlueLine.IndexOf(start); sign * i < sign * GoTo; i += sign)
                        bezie.Points.Add(BlueLine[i].coord);
                    bezie.Points.Add(BlueLine[GoTo].coord);

                    GoTo = RedLine.FindIndex(st => st.Name == "Хрещатик");

                    sign = Math.Sign(RedLine.IndexOf(finish) - GoTo);
                    for (int i = GoTo; sign * i < sign * RedLine.IndexOf(finish); i += sign)
                        bezie.Points.Add(RedLine[i].coord);
                    bezie.Points.Add(finish.coord);
                }

                else
                {
                    GoTo = BlueLine.FindIndex(st => st.Name == "Площа Льва Толстого");
                    int sign = Math.Sign(GoTo - BlueLine.IndexOf(start));
                    for (int i = BlueLine.IndexOf(start); sign * i < sign * GoTo; i += sign)
                        bezie.Points.Add(BlueLine[i].coord);
                    bezie.Points.Add(BlueLine[GoTo].coord);

                    GoTo = GreenLine.FindIndex(st => st.Name == "Палац спорту");

                    sign = Math.Sign(GreenLine.IndexOf(finish) - GoTo);
                    for (int i = GoTo; sign * i < sign * GreenLine.IndexOf(finish); i += sign)
                        bezie.Points.Add(GreenLine[i].coord);
                    bezie.Points.Add(finish.coord);
                }
            }

            else
            {
                if (finish.Line == "Green")
                {
                    int sign = Math.Sign(GreenLine.IndexOf(finish) - GreenLine.IndexOf(start));
                    for (int i = GreenLine.IndexOf(start); sign * i < sign * GreenLine.IndexOf(finish); i += sign)
                        bezie.Points.Add(GreenLine[i].coord);
                    bezie.Points.Add(finish.coord);
                }

                else if (finish.Line == "Red")
                {
                    GoTo = GreenLine.FindIndex(st => st.Name == "Золоті Ворота");
                    int sign = Math.Sign(GoTo - GreenLine.IndexOf(start));
                    for (int i = GreenLine.IndexOf(start); sign * i < sign * GoTo; i += sign)
                        bezie.Points.Add(GreenLine[i].coord);
                    bezie.Points.Add(GreenLine[GoTo].coord);

                    GoTo = RedLine.FindIndex(st => st.Name == "Театральна");

                    sign = Math.Sign(RedLine.IndexOf(finish) - GoTo);
                    for (int i = GoTo; sign * i < sign * RedLine.IndexOf(finish); i += sign)
                        bezie.Points.Add(RedLine[i].coord);
                    bezie.Points.Add(finish.coord);
                }

                else
                {
                    GoTo = GreenLine.FindIndex(st => st.Name == "Палац спорту");
                    int sign = Math.Sign(GoTo - GreenLine.IndexOf(start));
                    for (int i = GreenLine.IndexOf(start); sign * i < sign * GoTo; i += sign)
                        bezie.Points.Add(GreenLine[i].coord);
                    bezie.Points.Add(GreenLine[GoTo].coord);

                    GoTo = BlueLine.FindIndex(st => st.Name == "Площа Льва Толстого");

                    sign = Math.Sign(BlueLine.IndexOf(finish) - GoTo);
                    for (int i = GoTo; sign * i < sign * BlueLine.IndexOf(finish); i += sign)
                        bezie.Points.Add(BlueLine[i].coord);
                    bezie.Points.Add(finish.coord);
                }

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

            Canvas mainPanel = new Canvas();
            mainPanel.Width = 700;
            mainPanel.Height = 680;
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
    }
}
