using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;

namespace TravellingSalesmanAlgorithms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static DataTable dt;
        public Random ran = new Random();
        static public List<Tuple<int, int>> nodes;
        static public List<List<double>> adjacencyMatrix; //ger avstånd mellan två noder t.ex adjacencyMatrix[1][2] 
        static public List<double> lengths; //Används endast för att spara Brute Force väg kostnaderna, ineffektivt
        static public List<string> paths; //Används endast för att spara Brute Force vägarnas noder
        static public double minimumLength; //
        static public List<int> pathSequence;

        public static bool makeCircuit = false;

        public MainWindow()
        {
            InitializeComponent();
            AntalNoder.Text = "0";
        }

        private void DataGridKord_SelectionChanged(object sender, SelectionChangedEventArgs e){} //UI grejer
        private void Grid_Loaded(object sender, RoutedEventArgs e){}

        private void Place_Click(object sender, RoutedEventArgs e) //Ritar upp alla noder i kordinat fältet
        {
            nodes = new List<Tuple<int, int>>();
            canvas.Children.Clear();
            int row = 0;
            foreach (DataRow item in dt.Rows)
            {
                Rectangle rectangle = new Rectangle();
                rectangle.Stroke = Brushes.Black;
                rectangle.Width = 3;
                rectangle.Height = 3;
                try
                {
                    Canvas.SetLeft(rectangle, Convert.ToDouble(dt.Rows[row][0])-1);
                    Canvas.SetTop(rectangle, Convert.ToDouble(dt.Rows[row][1])-1);
                    nodes.Add(new Tuple<int, int>((int)dt.Rows[row][0], (int)dt.Rows[row][1]));
                    canvas.Children.Add(rectangle);
                }
                catch (Exception)
                {
                    
                }
                row++;               
            }
            MakeDistanceMatrix(); //beräknar och lägger till euclidianska avstånd till index, O(n²)
        }

        private void MakeDistanceMatrix() //Skapar avstånds matrisen som används för alla algoritmer
        {
            adjacencyMatrix = new List<List<double>>();
            for (int i = 0; i < nodes.Count(); i++)
            {
                List<double> rad = new List<double>();

                for (int e = 0; e < nodes.Count(); e++)
                {
                    rad.Add(Math.Sqrt(Math.Pow(Math.Abs((double)(nodes[i].Item1 - nodes[e].Item1)), 2) + Math.Pow(Math.Abs((double)(nodes[i].Item2 - nodes[e].Item2)), 2))); //Avståndsformel
                }
                adjacencyMatrix.Add(rad);
            }
            
        }

        private void Randomize_Click(object sender, RoutedEventArgs e) //Slumpar koordinatvärden
        {
            dt = new DataTable();
            DataColumn X = new DataColumn("X", typeof(int));
            DataColumn Y = new DataColumn("Y", typeof(int));
            DataColumn Grupp = new DataColumn("Grupp", typeof(int));
            dt.Columns.Add(X);
            dt.Columns.Add(Y);
            DataGridKord.ColumnWidth = new DataGridLength(1, DataGridLengthUnitType.Star);

            for (int i = 0; i < int.Parse(AntalNoder.Text); i++)
            {
                DataRow firstRow = dt.NewRow();
                firstRow[0] = ran.Next(0, 617);               
                firstRow[1] = ran.Next(0, 398);
                dt.Rows.Add(firstRow);
            }
            DataGridKord.ItemsSource = dt.DefaultView;
        }

        private void Draw_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (SlutIAnd.IsChecked == true)
            {
                makeCircuit = true;
            }
            else
            {
                makeCircuit = false;
            }

            if (algorithm.SelectedIndex > -1)
            {
                string s = algorithm.Text;                
                if (s == "O(n² * 2ⁿ)")
                {
                    DynamicProgramming.DPsetup();
                    stopwatch.Stop();
                    TidsVärde.Text = Math.Round(stopwatch.Elapsed.TotalMilliseconds).ToString();
                    KortasteVäg.Text = Math.Round(minimumLength).ToString();
                    System.Windows.Forms.MessageBox.Show(string.Join(",", DynamicProgramming.tour.ToArray()));
                    drawGraph(DynamicProgramming.tour);
                }
                else if (s == "O((n-1)!)")
                {
                    BruteForce.Setup(nodes.Count);
                    stopwatch.Stop();
                    TidsVärde.Text = Math.Round(stopwatch.Elapsed.TotalMilliseconds).ToString();
                    KortasteVäg.Text = Math.Round(lengths.Min()).ToString();
                    string pathToReiterate = paths[lengths.IndexOf(lengths.Min())].ToString();
                    System.Windows.Forms.MessageBox.Show(pathToReiterate);
                    drawGraph(pathToReiterate);
                }
                else if (s == "O(n)")
                {
                    AntColonyOptimization.SetupACO(int.Parse(AntalLoopar.Text), int.Parse(AntalMyror.Text), Convert.ToDouble(RhoVärde.Text), int.Parse(AlphaVärde.Text), int.Parse(BetaVärde.Text));
                    stopwatch.Stop();
                    TidsVärde.Text = Math.Round(stopwatch.Elapsed.TotalMilliseconds).ToString();
                    KortasteVäg.Text =Math.Round(AntColonyOptimization.bestLength).ToString();
                    System.Windows.Forms.MessageBox.Show(string.Join(",", AntColonyOptimization.bestTrail.ToArray()));
                    List<int> pathToReiterate = AntColonyOptimization.bestTrail.OfType<int>().ToList();
                    drawGraph(pathToReiterate); 
                }
            }
            
        }

        private void drawGraph(string path) //Ritar grafen, finns en för sträng och en för lista
        {            
            for (int i = 0; i < path.Length-1; i++)
            {
                Line line = new Line();
                line.X1 = nodes[Convert.ToInt32(new string(path[i], 1))].Item1;
                line.X2 = nodes[Convert.ToInt32(new string(path[i+1], 1))].Item1;
                line.Y1 = nodes[Convert.ToInt32(new string(path[i], 1))].Item2;
                line.Y2 = nodes[Convert.ToInt32(new string(path[i+1], 1))].Item2;
                SolidColorBrush brush = new SolidColorBrush(Colors.Black);
                line.StrokeThickness = 1;
                line.Stroke = brush;
                canvas.Children.Add(line);
            }
        }
        private void drawGraph(List<int> path)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Line line = new Line();
                line.X1 = nodes[path[i]].Item1;
                line.X2 = nodes[path[i + 1]].Item1;
                line.Y1 = nodes[path[i]].Item2;
                line.Y2 = nodes[path[i + 1]].Item2;
                SolidColorBrush brush = new SolidColorBrush(Colors.Black);
                line.StrokeThickness = 1;
                line.Stroke = brush;
                canvas.Children.Add(line);
            }
        }

    }
}
