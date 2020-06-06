using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TravellingSalesmanAlgorithms
{
    class BruteForce
    {
        public static void Setup(int a)
        {
            string str = "";
            for (int i = 1; i < a; i++)
            {
                str += i;
            }
            char[] arr = str.ToCharArray();
            MainWindow.lengths = new List<double>();
            MainWindow.paths = new List<string>();
            GetPer(arr);            
        }

        private static void Swap(ref char a, ref char b)
        {
            if (a == b) return;

            var temp = a;
            a = b;
            b = temp;
        }

        public static void GetPer(char[] list)
        {
            int x = list.Length - 1;
            GetPer(list, 0, x);
        }

        private static void GetPer(char[] list, int k, int m) //Genererar alla permutationer av chars
        {
            if (k == m)
            {
                string väg = "0";
                if (MainWindow.makeCircuit == true) //Lägger till nollan i slutet ifall det ska bli en hamilton krets
                {                    
                    väg += new string(list.ToArray());
                    väg += 0;                           
                    GetLength(väg);
                }
                else
                {
                    väg += new string(list.ToArray());
                    GetLength(väg);
                }                                
            }
            else
                for (int i = k; i <= m; i++)
                {
                    Swap(ref list[k], ref list[i]);
                    GetPer(list, k + 1, m);
                    Swap(ref list[k], ref list[i]);
                }
        }

        private static void GetLength(string path)
        {
            double pathDistance = 0;
            for (int i = 0; i < (path.Length-1); i++)
            {
                int p = Convert.ToInt32(new string(path[i], 1));
                int p2 = Convert.ToInt32(new string(path[i+1], 1));
                pathDistance += MainWindow.adjacencyMatrix[p][p2];
            }
            MainWindow.lengths.Add(pathDistance);
            MainWindow.paths.Add(path);
        }
    }
}
