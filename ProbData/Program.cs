using System;
using System.Collections.Generic;
using System.IO;

namespace ProbData
{
    static class Randoms
    {
        public static Random _rnd = new Random();
        static int getNextSingle()
        {
            return _rnd.Next() % 6 + 1;
        }

        public static int getNextSum(int n)
        {
            int S = 0;
            for (int i = 0; i < n; ++i)
            {
                S += getNextSingle();
            }
            return S;
        }

        public static int getNDice(int N)
        {
            return _rnd.Next() % N + 1;
        }
    }

    static class DataHolder
    {
        const int _nSize = 4000000;
        const int _nDice = 6;
        static int _maxValue = _nDice * 6;

        static List<int>[] _HiddenDice = new List<int>[_maxValue + 1];

        public static void MakeData()
        {
            List<int> hiddens = new List<int>();
            List<int> labels = new List<int>();

            for (int i = 0; i < _nSize; ++i)
            {
                int label = Randoms.getNDice(_nDice);
                int hidden = Randoms.getNextSum(label);

                hiddens.Add(hidden);
                labels.Add(label);
            }

            for (int k = 0; k < _maxValue + 1; ++k)
            {
                _HiddenDice[k] = new List<int>();
            }

            for (int i = 0; i < _nSize; ++i)
            {
                _HiddenDice[hiddens[i]].Add(labels[i]);
            }
        }

        public static int GetLabelForHidden(int hidden)
        {
            int len = _HiddenDice[hidden].Count;
            int rnd = Randoms._rnd.Next() % len;
            return _HiddenDice[hidden][rnd];
        }

        public static double[] getStat(int hidden)
        {
            double[] stat = new double[_nDice];
            foreach (int k in _HiddenDice[hidden])
            {
                stat[k - 1] += 1.0;
            }

            double sum = 0.0;
            foreach (double v in stat)
            {
                sum += v;
            }

            for (int i = 0; i < stat.Length; ++i)
            {
                stat[i] /= sum;
            }

            return stat;
        }
    }

    class Program
    {
        static string[] _labels = new string[] { "N", "R", "Q", "F", "B", "S" };
        public static double Formula(double[] x)
        {
            //y = (1/pi)*(2+2*x3)*(1/3)*(atan(20*exp(x5)*(x1-0.5+x2/6))+pi/2) + (1/pi)*(2+2*x4)*(1/3)*(atan(20*exp(x5)*(x1-0.5-x2/6))+pi/2);
            double pi = 3.14159265359;
            if (5 != x.Length)
            {
                Console.WriteLine("Formala error");
                Environment.Exit(0);
            }
            double y = (1.0 / pi);
            y *= (2.0 + 2.0 * x[2]);
            y *= (1.0 / 3.0);
            y *= Math.Atan(20.0 * Math.Exp(x[4]) * (x[0] - 0.5 + x[1] / 6.0)) + pi / 2.0;

            double z = (1.0 / pi);
            z *= (2.0 + 2.0 * x[3]);
            z *= (1.0 / 3.0);
            z *= Math.Atan(20.0 * Math.Exp(x[4]) * (x[0] - 0.5 - x[1] / 6.0)) + pi / 2.0;

            return y + z;
        }

        static void Main(string[] args)
        {
            DataHolder.MakeData();
            using (StreamWriter writer = new StreamWriter(@"..\..\..\..\Training.txt"))
            {
                int N = 10000;
                for (int i = 0; i < N; ++i)
                {
                    double[] x = new double[]
                    {
                        Randoms._rnd.Next() % 100 / 100.0,
                        Randoms._rnd.Next() % 100 / 100.0,
                        Randoms._rnd.Next() % 100 / 100.0,
                        Randoms._rnd.Next() % 100 / 100.0,
                        Randoms._rnd.Next() % 100 / 100.0
                    };

                    double y = Formula(x);
                    int z = (int)Math.Round(y / 2.6 * 35.0 + 1.0);
                    if (z < 1) z = 1;
                    if (z > 36) z = 36;
                    z = DataHolder.GetLabelForHidden(z);

                    String s = String.Format("{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}, {4:0.00}, {5}",
                        x[0], x[1], x[2], x[3], x[4], _labels[z - 1]);

                    writer.WriteLine(s);
                }
            }

            using (StreamWriter writer = new StreamWriter(@"..\..\..\..\Test.txt"))
            {
                writer.WriteLine("inputs: x1, x2, x3, x4, x5, probabilities for labels: N, R, Q, F, B, S");
                int N = 100;
                for (int i = 0; i < N; ++i)
                {
                    double[] x = new double[]
                    {
                        Randoms._rnd.Next() % 100 / 100.0,
                        Randoms._rnd.Next() % 100 / 100.0,
                        Randoms._rnd.Next() % 100 / 100.0,
                        Randoms._rnd.Next() % 100 / 100.0,
                        Randoms._rnd.Next() % 100 / 100.0
                    };

                    double y = Formula(x);
                    int z = (int)Math.Round(y / 2.6 * 35.0 + 1.0);
                    if (z < 1) z = 1;
                    if (z > 36) z = 36;
                    double[] stat = DataHolder.getStat(z);

                    String s = String.Format("{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}, {4:0.00}, {5:0.0000}, {6:0.0000}, {7:0.0000}, {8:0.0000}, {9:0.0000}, {10:0.0000}",
                        x[0], x[1], x[2], x[3], x[4], stat[0], stat[1], stat[2], stat[3], stat[4], stat[5]);

                    writer.WriteLine(s);
                }
            }
        }
    }
}
