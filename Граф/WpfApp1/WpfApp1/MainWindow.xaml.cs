using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private ClassGraph1 Graph;
        private Canvas canvasNodes;
       
        public MainWindow()
        {
            InitializeComponent();
            Graph = new ClassGraph1();
          
            ListBoxNodes.ItemsSource = Graph.Nodes;
            ListBoxEdges.ItemsSource = Graph.Edges;         

        }

        //проверка спауна
        private void canvasNodes_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Canvas canvasNodes = sender as Canvas;
                if (canvasNodes != null)
                {
                    Point point = e.GetPosition(canvasNodes);
                    Graph.AddNode(point.X, point.Y);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private bool isDraggingNode = false;
        private Point? previousPoint = null;
        private Node firstNode = null;

        List<int> Svyzi = new List<int> { };
        //спаун связей и узлов
        private void gridNode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid != null)
            {
                grid.CaptureMouse();

                if (radioAddNode.IsChecked == true)
                {
                    isDraggingNode = true;
                    previousPoint = e.GetPosition(canvasNodes);
                }

                if (radioAddEdge.IsChecked == true)
                {
                    Svyzi.Add(1);
                    Node currentNode = null;
                    currentNode = grid.DataContext as Node;
                    if (firstNode == null)
                    {
                        
                        firstNode = currentNode;
                        
                    }
                    else
                    {
                        Svyzi.Add(0);
                        Graph.AddEdge(firstNode, currentNode);
                        firstNode = null;
                    }

                }
            }
        }

        // передвижение точек
        private void gridNode_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingNode)
            {
                Grid element = sender as Grid;
                if (element != null)
                {
                    Node node = element.DataContext as Node;
                    
                    if (node != null && previousPoint.HasValue)
                    {
                        Point point = e.GetPosition(canvasNodes);

                        double xDiff = point.X - previousPoint.Value.X;
                        double yDiff = point.Y - previousPoint.Value.Y;

                        node.X += xDiff;
                        node.Y += yDiff;

                        node.CentreX += xDiff;
                        node.CentreY += yDiff;

                        previousPoint = point;
                    }
                }
              
            }
        }

        private void gridNode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid != null)
            {
                grid.ReleaseMouseCapture();
                isDraggingNode = false;
                previousPoint = null;
            }
        }

       private void canvasNodes_Loaded(object sender, RoutedEventArgs e)
       {
            canvasNodes = sender as Canvas;
       }    


        // матрица смежности
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            int[,] Table = Graph.BuildAdjacencyMatrix(Graph.Nodes, Graph.Edges);


            if (Table.GetLength(0) == 0) 
            {
                BoxMatrix.Text = string.Empty;
                BoxMatrix.Background = Brushes.IndianRed;
                return; 
            }

            BoxMatrix.Background = Brushes.Transparent;

            List<int> myStrings = new List<int> { };

            for (var i = 0; i < Table.GetLength(0); i++)
            {
                for (var j = 0; j < Table.GetLength(1); j++)
                {
                    myStrings.Add(Table[i, j]);
                }
            }

            int uzel = 1, uzel_uzel = 1, end = (Graph.nextIndex - 1) * 2, start = 0;
            string matrix = string.Empty, uzel1point = string.Empty, myIntsString = string.Join(" ", myStrings) + " ";
         
            uzel1point += "   | ";
            for (var i = 0; i < Graph.nextIndex - 1; i++)
            {               
                uzel1point += $"{uzel_uzel} ";
                uzel_uzel++;
            }

            for (var i = 0; i < Graph.nextIndex - 1; i++)
            {
                matrix += $"{uzel} | " + myIntsString.Substring(start, end) + "\n";
                start += end;
                uzel++;
            }
            uzel1point += "\n";

            BoxMatrix.Text = uzel1point + matrix;
           
        }

        // максимальный поток
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

          if(Graph.BuildAdjacencyMatrix(Graph.Nodes, Graph.Edges).GetLength(0) != 0)
          {
                FlowBox.Background = Brushes.Transparent;
                FlowBox.Text = $"От 1 до {Graph.BuildAdjacencyMatrix(Graph.Nodes, Graph.Edges).GetLength(0)} узла\nРавен: " + Convert.ToString(MaxFlow(Graph.BuildAdjacencyMatrix(Graph.Nodes, Graph.Edges), 0, Graph.BuildAdjacencyMatrix(Graph.Nodes, Graph.Edges).GetLength(0) - 1));
          }
            else
            {
                FlowBox.Text = string.Empty;
                FlowBox.Background = Brushes.IndianRed;
            }   
          
        }

        private int MaxFlow(int[,] graph, int source, int sink)
       {
            int[] parent = new int[graph.GetLength(0)];
            int maxFlow = 0;
            while (Bfs(graph, source, sink, parent))
            {
                int pathFlow = int.MaxValue;
                for (int v = sink; v != source; v = parent[v])
                {
                    int u = parent[v];
                    pathFlow = Math.Min(pathFlow, graph[u, v]);
                }
                for (int v = sink; v != source; v = parent[v])
                {
                    int u = parent[v];
                    graph[u, v] -= pathFlow;
                    graph[v, u] += pathFlow;
                }
                maxFlow += pathFlow;
            }
            return maxFlow;
       }

       private  bool Bfs(int[,] graph, int source, int sink, int[] parent)
       {
            bool[] visited = new bool[graph.GetLength(0)];
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(source);
            visited[source] = true;
            parent[source] = -1;
            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                for (int v = 0; v < graph.GetLength(0); v++)
                {
                    if (!visited[v] && graph[u, v] > 0)
                    {
                        queue.Enqueue(v);
                        visited[v] = true;
                        parent[v] = u;
                    }
                }
            }
            return visited[sink];
       }


        //алгоритм для обхода в глубь графа

        private bool[] visited = new bool[1000];
         
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
             if(Graph.BuildAdjacencyMatrix(Graph.Nodes, Graph.Edges).GetLength(0) != 0)
            {
                listBox1.Background = Brushes.Transparent;
                for (int i = 0; i < visited.Length; i++)
                {
                    visited[i] = false;
                }
                listBox1.Items.Clear();
                DFS(0);
            }
            else
            {
                listBox1.Items.Clear();
                listBox1.Background = Brushes.IndianRed;
            }

        }

        private void DFS(int i)
        {
            int[,] adjMatrix = Graph.BuildAdjacencyMatrix(Graph.Nodes, Graph.Edges);
            visited[i] = true;
            listBox1.Items.Add("Узел: " + (i+1).ToString());
            for (int j = 0; j < adjMatrix.GetLength(1); j++)
            {
                if (adjMatrix[i, j] == 1 && !visited[j])
                {
                    DFS(j);
                }
            }
        }


        // Определение циклов

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            int[,] adjacencyMatrix = Graph.BuildAdjacencyMatrix(Graph.Nodes, Graph.Edges);



            BoxCycle.Text = string.Empty;


            if (Graph.BuildAdjacencyMatrix(Graph.Nodes, Graph.Edges).GetLength(0) != 0)
            {
                BoxCycle.Background = Brushes.Transparent;

                // Эйлеров цикл
                bool euler = true;
                for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
                {
                    int degree = 0;
                    for (int j = 0; j < adjacencyMatrix.GetLength(1); j++)
                    {
                        degree += adjacencyMatrix[i, j];
                    }
                    if (degree % 2 != 0)
                    {
                        euler = false;
                        break;
                    }
                }

                if (euler)
                {
                    BoxCycle.Text += "Граф содержит \nэйлеров цикл.\n\n";
                }
                else
                {
                    BoxCycle.Text += "Граф не содержит \nэйлеров цикл.\n\n";
                }


                // Гамлетов цикл

                if (HamiltonianCycle(adjacencyMatrix))
                {
                    BoxCycle.Text += "Граф содержит \nгамильтонов цикл.\n\n";
                }
                else
                {
                    BoxCycle.Text += "Граф не содержит \nгамильтонов цикл.\n\n";
                }
            }
            else
            {
                BoxCycle.Text = string.Empty;
                BoxCycle.Background = Brushes.IndianRed;
            } 

        }

         bool HamiltonianCycle(int[,] graph)
         {
            int[] path = new int[graph.GetLength(0)];
            for (int i = 0; i < path.Length; i++)
            {
                path[i] = -1;
            }
            path[0] = 0;
            if (!HamiltonianCycleUtil(graph, path, 1))
            {
                return false;
            }
            return true;
         }

         bool HamiltonianCycleUtil(int[,] graph, int[] path, int pos)
         {
            if (pos == graph.GetLength(0))
            {
                if (graph[path[pos - 1], path[0]] == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            for (int v = 1; v < graph.GetLength(0); v++)
            {
                if (IsSafe(v, graph, path, pos))
                {
                    path[pos] = v;
                    if (HamiltonianCycleUtil(graph, path, pos + 1))
                    {
                        return true;
                    }
                    path[pos] = -1;
                }
            }
            return false;
         }

        static bool IsSafe(int v, int[,] graph, int[] path, int pos)
        {
            if (graph[path[pos - 1], v] == 0)
            {
                return false;
            }
            for (int i = 0; i < pos; i++)
            {
                if (path[i] == v)
                {
                    return false;
                }
            }
            return true;
        }


        // очистка
        void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Graph.Nodes.Clear();
            Graph.Edges.Clear();
            BoxCycle.Text = string.Empty;
            FlowBox.Text = string.Empty;
            BoxMatrix.Text = string.Empty;
            listBox1.Items.Clear();

            BoxMatrix.Background = Brushes.Transparent;
            FlowBox.Background = Brushes.Transparent;
            BoxCycle.Background = Brushes.Transparent;
            listBox1.Background = Brushes.Transparent;

            Graph = null;

            InitializeComponent();
            Graph = new ClassGraph1();
            ListBoxNodes.ItemsSource = Graph.Nodes;
            ListBoxEdges.ItemsSource = Graph.Edges;

        }
    }

}
