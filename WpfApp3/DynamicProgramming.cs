using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace TravellingSalesmanAlgorithms
{
    class DynamicProgramming
    {
        private static int N; //Antal noder
        static int start;
        static int FINISHED_STATE;
        public static double minTourCost = double.PositiveInfinity;
        private static List<List<double>> distance;
        public static List<int> tour; 

        static void solve()
        {

            int END_STATE = (1 << N) - 1; //Alla noder är besökta och alla bitar(I nodernas index) är flippade
            double[,] memo = new double[N,1 << N]; //Om programmet startas som 32 bitar tar minnet slut vid n=>24
            
            for (int end = 0; end < N; end++) //Börjat med att lägga till alla fall med två bitar flippade; lägger till alla kanter utifrån startnoden i memot
            {
                if (end == start) continue;
                memo[end,(1 << start) | (1 << end)] = distance[start][end]; //Binär operatorn '|' (OR) flippar biten om den är flippad i antingen HL eller VL       
            }

            for (int r = 3; r <= N; r++) //Fortsätter med att sätta in alla fall där tre till N noder besöks; tre till N bitar är ettor
            {
                foreach (int subset in combinations(r, N))
                {
                    if (notIn(start, subset)) continue; //Kollar om startnoden finns i delmängden, om den inte finns hoppar den över fallet
                    for (int next = 0; next < N; next++)
                    {
                        if (next == start || notIn(next, subset)) continue; //next måste vara i delmängden, den får däremot inte vara startnoden
                        int subsetWithoutNext = subset ^ (1 << next); //Binär operatorn '^'(XOR) används för att skapa delmängden som inte har en etta flippad på index next, detta gör att vi nu kan kolla upp bästa partiella väg i memo
                        double minDist = double.PositiveInfinity;
                        for (int end = 0; end < N; end++)
                        {
                            if (end == start || end == next || notIn(end, subset)) continue; //end är ändnoden i subsetWithoutNext, kan inte vara startnord eller en nod i delmängden hitils eller nästa nod
                            double newDistance = memo[end,subsetWithoutNext] + distance[end][next]; //subsetWithoutNext är känt sedan innan i memo table, det som behövs räknas ut är avståndet från senaste noden till den nya noden
                            if (newDistance < minDist) //sparar vägen om den är den minsta hittad hittils
                            {
                                minDist = newDistance;
                            }
                        }
                        memo[next,subset] = minDist; //sparar minsta avståndet i vägen hittils i memot
                    }
                }
            }           
            int lastIndex = start; //Används i nästnästa loop för att återbygga vägen
            for (int i = 0; i < N; i++) //Kollar i memot vilken ändnod som ger kortast total väg
            {
                if (i == start) continue;
                double tourCost;
                if (MainWindow.makeCircuit == true) //Lägger till kostnaden tillbaka till startnoden för en hamilton krets
                {
                    tourCost = memo[i, END_STATE] + distance[i][start];
                }
                else //Skapar hamilton väg
                {
                    tourCost = memo[i, END_STATE];
                }            
                if (tourCost < minTourCost) //Jämför om den nya vägen är den kortaste och sparar den nya om den är kortare
                {
                    minTourCost = tourCost;
                    lastIndex = i;
                }
            }

            int state = END_STATE;           
            if (MainWindow.makeCircuit == true)
            {
                lastIndex = start;
                tour.Add(start); //Tour är en lista som beskriver vilka noder har besökts i kortaste väg, startnoden läggs bara till i slutet om en hamilton krets ska skapas
            }
           
            for (int i = 1; i < N; i++) // Återbygger kortaste vägen genom att kolla i memot och lägga till vilka noder som besöktes i tour
            {

                int index = -1;
                for (int j = 0; j < N; j++)
                {
                    if (j == start || notIn(j, state)) continue;
                    if (index == -1) index = j;
                    double prevDist = memo[index,state] + distance[index][lastIndex];
                    double newDist = memo[j,state] + distance[j][lastIndex];
                    if (newDist < prevDist)
                    {
                        index = j;
                    }
                }

                tour.Add(index);
                state = state ^ (1 << index);
                lastIndex = index;
            }
            tour.Add(start); //start positionen läggs till     
            tour.Reverse(); //Listan vänds till orndning startnod besökt till ändnod
        }

        private static bool notIn(int elem, int subset) //Binär operatorn '&'(And) kollar om poistionens(elem) etta är flippad i delmängden, om den är det så sätts VL till något större en 0 och bool värdet false returneras, annars returneras true
        {
            return ((1 << elem) & subset) == 0;
        }

        static List<int> combinations(int r, int n) //Genererar alla delmängder vid storlek n där r bits är flippade
        {
            List<int> subsets = new List<int>();
            combinations(0, 0, r, n, subsets);
            return subsets;
        }

        private static void combinations(int set, int at, int r, int n, List<int> subsets) //För att hitta alla kombinationer av storlek går vi rekursivt tillbaka tills r element är valda.
        {

            int elementsLeftToPick = n - at;
            if (elementsLeftToPick < r) return; //Returnera om det inte finns några mer element att välja

            if (r == 0) //r element är valda och med det en giltig delmängd
            {
                subsets.Add(set);
            }
            else
            {
                for (int i = at; i < n; i++)
                {
                    set ^= (1 << i); //Försök att inkludera elementet

                    combinations(set, i + 1, r - 1, n, subsets);

                    set ^= (1 << i); //Gå tillbaka och försök instansen då elementet inte inkluderades
                }
            }
        }

        public static void DPsetup()
        {
            N = MainWindow.nodes.Count();
            start = 0;
            minTourCost = double.PositiveInfinity;
            FINISHED_STATE = (1 << N) - 1; //State där alla noder är besökta
            distance = MainWindow.adjacencyMatrix;
            tour = new List<int>();
            solve();
            MainWindow.minimumLength = minTourCost;

        }
    }
}