using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravellingSalesmanAlgorithms
{
    class AntColonyOptimization
    {
        private static int N; //Antal noder
        static int numAnts; //Antal myror
        static int maxTime; //Antal gånger programmet loopar igenom och updaterar
        static int alpha; //feromon viktning
        static int beta; //kant längd viktning
        static double rho; //Feromon avdunstnings faktor
        static double Q = 100.0000; //feromon ökning faktor

        static Random random = new Random();
        static List<List<double>> dists; //Avstånds matrisen
        static List<List<int>> ants; //Myrorna, varje myra är en lista utav de noder myran besökt
        public static List<int> bestTrail; //Sparar den hittils bästa vägens nod index
        public static double bestLength; //Sparar den hittils bästa vägens kostnad
        static double[,] pheromones; //Lagrar värderna för varje kants feromoner

        public static void SetupACO(int loops, int myror, double Rho, int Alpha, int Beta)
        {
            rho = Rho;
            beta = Beta;
            alpha = Alpha;
            maxTime = loops;
            numAnts = myror;

            N = MainWindow.nodes.Count();
            ants = new List<List<int>>();
            dists = MainWindow.adjacencyMatrix;
            pheromones = InitPheromones();
            UpdateAnts();
            bestTrail = BestTrail();
            bestLength = Length(bestTrail);
            int time = 0;
            while (time < maxTime)
            {
                UpdateAnts();
                UpdatePheromones();

                List<int> currBestTrail = BestTrail();
                double currBestLength = Length(currBestTrail);
                if (currBestLength < bestLength)
                {
                    bestLength = currBestLength;
                    bestTrail = currBestTrail;
                }
                ++time;
            }


        }

        private static double Length(List<int> trail) //Beräknar längden av en väg i trail
        {
            double result = 0;

            if (MainWindow.makeCircuit == true) //lägger till kostnaden tillbaka till starten ifall det är en krets
            {
                for (int i = 0; i < N; i++)
                {
                    result += dists[trail[i]][trail[i + 1]];
                }
            }
            else
            {
                for (int i = 0; i < N - 1; i++) //Annars behöver bara kolla N-1 fall
                {
                    result += dists[trail[i]][trail[i + 1]];
                }
            }
            return result;
        }

        static double[,] InitPheromones() //initialiserar feromoner genom att lägga till grund värdet 1 i varje kant, görs bara en gång.
        {
            double[,] pheromones = new double[N, N];
            for (int i = 0; i < pheromones.GetLength(0); ++i)
                for (int j = 0; j < pheromones.GetLength(1); ++j)
                    pheromones[i, j] = 1.00; //Lägger till basvärdet 1 till alla då myrorna ännu ej släppts lös      
            return pheromones;
        }

        private static List<int> BestTrail() //Går igenom alla myrorna, beräknar deras väg, och lagrar den hittils bästa vägen
        {
            double bestLength = Length(ants[0]);
            int idxBestLength = 0;
            for (int k = 1; k < ants.Count; k++)
            {
                double len = Length(ants[k]);
                if (len < bestLength)
                {
                    bestLength = len;
                    idxBestLength = k;
                }
            }
            List<int> bestTrail_Renamed = ants[idxBestLength];
            return bestTrail_Renamed;
        }

        static void UpdateAnts() //Skapar nu nya vägar och denna gången använder de nya sannolikheterna som kalkuleras med de nya feromon värderna
        {
            for (int k = 0; k < numAnts; ++k) //k = index för rad av myror
            {
                int start = 0;
                List<int> newTrail = BuildTrail(k, start);
                if (ants.Count != numAnts)
                {
                    ants.Add(newTrail);
                }
                else
                {
                    ants[k] = newTrail;
                }

            }
        }

        static List<int> BuildTrail(int k, int start) //Förbredelse till att skapa ny väg
        {
            List<int> trail = new List<int>();
            bool[] visited = new bool[N+1]; //sparar vilka noder som är besökta då vi inte vill besöka samma nod mer än en gång
            trail.Add(start);
            visited[start] = true;
            if (MainWindow.makeCircuit == true)
            {
                visited[trail.Count - 1] = true; //Ser till att vägen inte går tillbaka till startnoden för tidigt
            }
            for (int i = 0; i < N - 1; ++i) //Eftersom det finns en startposition finns det bara N-1 noder kvar att besöka
            {
                int nodeX = trail[i];
                int next = NextNode(k, nodeX, visited);
                trail.Add(next);
                visited[next] = true;
            }
            if (MainWindow.makeCircuit == true) //Lägger tillbaks starten i slutet om det ska vara en krets
            {
                trail.Add(start);
            }
            return trail;
        }

        static int NextNode(int k, int nodeX, bool[] visited) //Slumpar vilken väg att gå med hjälp av sannolikhetsvärdena
        {
            double[] probs = MoveProbs(k, nodeX, visited);

            double[] cumul = new double[probs.Length + 1];
            for (int i = 0; i < probs.Length; i++) //Skapar roulette hjul (Se 6.3.1)
            {
                cumul[i + 1] = cumul[i] + probs[i];
            }
            cumul[cumul.Length - 1] = 1;

            double p = random.NextDouble(); //Slumpar värde mellan 0.0 och 1.0

            for (int i = 0; i < cumul.Length - 1; i++)
            {
                if (p >= cumul[i] && p < cumul[i + 1]) //Kollar i vilket intervall i roulette hjulet p hamnar i och returnerar vilken den nästa noden ska vara
                {
                    return i;
                }
            }

            throw new Exception();
        }

        static double[] MoveProbs(int k, int nodeX, bool[] visited) //Skapar sannoliksvärden med hjälp av formeln
        {
            double[] taueta = new double[N]; //Täljare i sannolikhets formeln        
            double sum = 0.0; //Nämnare i sannolikhets formeln (likadan för alla sannolikhets beräkningar i vägen)
            for (int i = 0; i < taueta.Length; ++i)
            {
                if (i == nodeX)
                    taueta[i] = 0.0; //Sätter sannolikheten till att den går till sig själv till 0
                else if (visited[i] == true)
                    taueta[i] = 0.0; //Får inte gå till någon den har varit på, så sätter sannolikheten på de besökta till 0
                else
                {
                    taueta[i] = Math.Pow(pheromones[nodeX, i], alpha) * Math.Pow((1.0 / dists[nodeX][i]), beta);
                    if (taueta[i] < 0.0001) //Avrundning
                        taueta[i] = 0.0001;
                    else if (taueta[i] > (double.MaxValue / (N * 100))) //Avrunding
                        taueta[i] = double.MaxValue / (N * 100);
                }
                sum += taueta[i];
            }

            double[] probs = new double[N];
            for (int i = 0; i < probs.Length; ++i)
                probs[i] = taueta[i] / sum; //Slutligen beräknas alla sannolikheter
            return probs;
        }

        private static void UpdatePheromones() //Updaterar feromoner enligt formeln.
        {
            for (int i = 0; i < pheromones.GetLength(0); i++)
            {
                for (int j = i + 1; j < pheromones.GetLength(1); j++)
                {
                    for (int k = 0; k < ants.Count; k++)
                    {
                        double length = Length(ants[k]);
                        double decrease = (1.0000 - rho) * pheromones[i, j];
                        double increase = 0.0000;
                        if (EdgeInTrail(i, j, ants[k]) == true)
                        {
                            increase = (Q / length);
                        }
                        pheromones[i, j] = decrease + increase;

                        pheromones[j, i] = pheromones[i, j];
                    }
                }
            }

        }

        private static bool EdgeInTrail(int nodeX, int nodeY, List<int> trail) //Kollar så att kanten från i till j är i trail[], annars läggs ingen ökning på feromon värdet enligt formeln
        {
            int lastIndex = N - 1;
            int idx = trail.IndexOf(nodeX);

            if (MainWindow.makeCircuit == true) //Lägger till feromoner på vägen tillbaka om det är en krets
            {
                if (idx == 0 && trail[lastIndex] == nodeY)
                {
                    return true;
                }
                else if (idx == lastIndex && trail[0] == nodeY)
                {
                    return true;
                }
            }

            if (idx == 0 && trail[1] == nodeY) //För att kanten ska vara i trail[] måste i och j vara efterförljande
            {
                return true;
            }
            if (idx == 0)
            {
                return false;
            }
            else if (idx == lastIndex && trail[lastIndex - 1] == nodeY)
            {
                return true;
            }
            if (idx == lastIndex)
            {
                return false;
            }
            else if (trail[idx - 1] == nodeY)
            {
                return true;
            }
            else if (trail[idx + 1] == nodeY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



    }
}
