using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PushRelabel
{
    public partial class MainWindow : Window
    {
        int v_max = 10;
        int v_num = 0;
        List<Label> vertexes = new List<Label>();
        List<TextBlock> caps = new List<TextBlock>();
        List<Shape> edges = new List<Shape>();
        List<string> edges_u = new List<string>();
        List<string> v_names = new List<string>();
        Graph g;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateGraph_ButtonClick(object sender, RoutedEventArgs e)
        {
            if(SubMenuPanel.Visibility==Visibility.Collapsed)
            {
                SubMenuPanel.Visibility = Visibility.Visible;
                s_.Visibility = Visibility.Visible;
                Canvas.SetLeft(t_, c.ActualWidth-80);
                t_.Visibility = Visibility.Visible;
            }
            else
            {
                foreach (Label v in vertexes){c.Children.Remove(v);}
                vertexes.Clear();
                foreach (Shape sh in edges){c.Children.Remove(sh);}
                edges.Clear();
                foreach (TextBlock cap in caps){c.Children.Remove(cap);}
                caps.Clear();
                edges_u.Clear();
                v_names.Clear();
                v_num = 0;
            }
            g = new Graph(2);
            b1.IsEnabled = false;
            res_.Text = "";        
            v_names.Add("s");
            v_names.Add("t");
        }

        private void AddVertex_ButtonClick(object sender, RoutedEventArgs e)
        {
            if(v_num<v_max)
            { 
                v_num++;
                Label v_label = new Label();
                v_label.Content = v_num;
                v_label.MouseMove += Vertex_MouseMove;
                vertexes.Add(v_label);
                c.Children.Add(v_label);
                Canvas.SetZIndex(v_label, 100);
                v_names.Add(v_num.ToString());
                Canvas.SetLeft(v_label, 0);
                Canvas.SetTop(v_label, 0);

                g = new Graph(v_num+2);
            }
            else
            {
                MessageBox.Show("Досягнута максимальна кількість вершин!..");
            }
        }

        private void AddEdge_ButtonClick(object sender, RoutedEventArgs e)
        {
            string from_text = from_.Text, to_text = to_.Text, cap_text = cap_.Text;
            string edge_name = from_text + " - " + to_text;
            if(v_names.Contains(from_text) && v_names.Contains(to_text))
            {
                int n;
                if(int.TryParse(cap_text, out n)==true && n >=0)
                {
                    if (!edges_u.Contains(edge_name))
                    {
                        edges_u.Add(edge_name);
                        edge_name = to_text + " - " + from_text;
                        edges_u.Add(edge_name);

                        if (b1.IsEnabled == false) b1.IsEnabled = true;
                        Point from = new Point();
                        Point to = new Point();
                        foreach (var item in c.Children)
                        {
                            if (item.GetType() == typeof(Label))
                            {
                                if ((item as Label).Content.ToString() == from_text)
                                {
                                    from.X = Canvas.GetLeft(item as Label) + 35;
                                    from.Y = Canvas.GetTop(item as Label) + 35;
                                    (item as Label).MouseMove -= Vertex_MouseMove;
                                }
                            }
                        }
                        foreach (var item in c.Children)
                        {
                            if (item.GetType() == typeof(Label))
                            {
                                if ((item as Label).Content.ToString() == to_text)
                                {
                                    to.X = Canvas.GetLeft(item as Label) + 35;
                                    to.Y = Canvas.GetTop(item as Label) + 35;
                                    (item as Label).MouseMove -= Vertex_MouseMove;
                                }
                            }
                        }

                        Shape s_tmp = DrawLinkArrow(from, to);
                        TextBlock cap_label = new TextBlock();
                        cap_label.Text = cap_text;
                        cap_label.FontSize = 20;
                        caps.Add(cap_label);
                        c.Children.Add(cap_label);
                        Canvas.SetLeft(cap_label, (from.X + to.X) / 2 - cap_label.ActualWidth / 2);
                        Canvas.SetTop(cap_label, (from.Y + to.Y) / 2 - 40);
                        edges.Add(s_tmp);
                        c.Children.Add(s_tmp);
                        Canvas.SetZIndex(s_tmp, 0);

                        if (from_text == "s") from_text = "0";
                        if (to_text == "t") to_text = (v_num + 1).ToString();
                        g.addEdge(Int32.Parse(from_text), Int32.Parse(to_text), Int32.Parse(cap_text));
                    }
                    else MessageBox.Show($"Ребро {edge_name} вже існує!..");
                }
                else MessageBox.Show($"Пропускна спроможність задана неправильно!..");
            }
            else MessageBox.Show($"Такої верштни не існує!..");

            from_.Text = to_.Text = cap_.Text="";
        }

        private void GetMaxFlow_ButtonClick(object sender, RoutedEventArgs e)
        {
            int s = 0, t = v_num + 1;
            res_.Text = "Максимальний потік у мережі становить " + g.getMaxFlow(s, t).ToString();
            res_.Visibility = Visibility.Visible;
            about.Visibility = Visibility.Collapsed;
        }

        private void Vertex_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Source is Label)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point p = e.GetPosition(c);
                    if(p.X - (sender as Label).ActualWidth / 2 > 0 && p.X + (sender as Label).ActualWidth / 2 < c.ActualWidth)
                        Canvas.SetLeft((sender as Label), p.X - (sender as Label).ActualWidth / 2);
                    if (p.Y - (sender as Label).ActualHeight / 2 > 0 && p.Y + (sender as Label).ActualHeight / 2 < c.ActualHeight)
                        Canvas.SetTop((sender as Label), p.Y - (sender as Label).ActualHeight / 2);
                    (sender as Label).CaptureMouse();
                }
                else
                {
                    (sender as Label).ReleaseMouseCapture();
                }
            }
        }

        private static Shape DrawLinkArrow(Point p1, Point p2)
        {
            GeometryGroup lineGroup = new GeometryGroup();
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            Point p = new Point(p1.X + ((p2.X - p1.X) / 2), p1.Y + ((p2.Y - p1.Y) / 2));
            pathFigure.StartPoint = p;

            Point lpoint = new Point(p.X + 6, p.Y + 15);
            Point rpoint = new Point(p.X - 6, p.Y + 15);

            LineSegment seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);

            LineSegment seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);

            LineSegment seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);

            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            path.Data = lineGroup;
            path.StrokeThickness = 2;
            path.Stroke = path.Fill = Brushes.Black;

            return path;
        }

        private void OpenManual_ButtonClick(object sender, RoutedEventArgs e)
        {
            res_.Visibility = Visibility.Collapsed;
            about.Visibility = Visibility.Visible;
            num_text.Text = "1.";
            about_text.Text = "Натисніть <Створити мережу>, щоб додати джерело і сток графу...";
            next.Content = "Далі";
        }

        private void Next_ButtonClick(object sender, RoutedEventArgs e)
        {
            switch(num_text.Text)
            {
                case "1.":
                    {
                        num_text.Text = "2.";
                        about_text.Text = "Натисніть <Додати вершину>, щоб додати необхідну кількість вершин графу. Максимальна кількість вершин - 10 (12 з джерелом та стоком). Пересуньте вершину на бажану позицію за допомогою миші. Зауважте, що після додання ребра до вершини, змінити її позицію буде неможливо...";
                    }
                    break;
                case "2.":
                    {
                        num_text.Text = "3.";
                        about_text.Text = "Щоб поєднати вершини у дуги (ребра), заповніть поля u - v, cap = x, де u, v - імена вершин, що будуть з'єднані дугою у відповідному напрямку, cap = x - пропускна здатність дуги (має бути цілочисельною). Тепер натисніть <Додати ребро>. Для коректної роботи програми створений граф має містити хоча б один цільний шлях від s до t та не містити циклів...";
                    }
                    break;
                case "3.":
                    {
                        num_text.Text = "4.";
                        about_text.Text = "Якщо ви помилилися при створенні мережі натисніть <Створити мережу> і почніть спочатку...";
                    }
                    break;
                case "4.":
                    {
                        num_text.Text = "5.";
                        about_text.Text = "Натисніть <Обчислити>, щоб розрахувати максимальний потік у створеній мережі. Ця функція доступна, якщощ граф містить хоча б одне ребро.";
                        next.Content = "Зрозуміло!";
                    }
                    break;
                case "5.":
                    {
                        res_.Visibility = Visibility.Visible;
                        about.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }
    }
}
