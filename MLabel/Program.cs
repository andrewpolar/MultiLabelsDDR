using System;
using System.IO;
using System.Collections.Generic;

namespace MLabel
{
    class Program
    {
        static double[] GetStat(double[] target)
        {
            int[] stat = new int[6];
            foreach (double x in target)
            {
                int i = (int)(Math.Round(x) + 0.1);
                if (i < 1) i = 1;
                if (i > 6) i = 6;
                ++stat[i - 1];
            }

            double[] probs = new double[6];
            double sum = 0.0;
            foreach (int k in stat)
            {
                sum += (double)(k);
            }

            for (int i = 0; i < 6; ++i)
            {
                probs[i] = (double)(stat[i]) / sum;
            }

            return probs;
        }

        static double KLDiv(double[] p1, double[] p2)
        {
            double kl = 0.0;
            for (int i = 0; i < p1.Length; ++i)
            {
                if (p1[i] > 0.0)
                {
                    kl += p1[i] * Math.Log(p1[i] / (p2[i] + 0.01));
                }
            }
            return Math.Abs(kl);
        }

        static double[] ResortLabels(int[] resorter, double[] x)
        {
            double[] y = new double[x.Length];
            for (int i = 0; i < x.Length; ++i)
            {
                y[resorter[i]] = x[i];
            }
            return y;
        }

        static int[] MakeLabelResorter(List<string> actual, string header)
        {
            string[] blocks = header.Split(":");
            string[] data = blocks[2].Split(",");
            string[] original = new string[actual.Count];
            int cnt = 0;
            for (int i = data.Length - 6; i < data.Length; ++i)
            {
                original[cnt++] = data[i].Trim();
            }

            List<int> reorder = new List<int>();
            foreach (string s in original)
            {
                int counter = 0;
                foreach (string q in actual)
                {
                    if (q.Contains(s))
                    {
                        reorder.Add(counter);
                        break;
                    }
                    counter++;
                }
            }
            return reorder.ToArray();
        }

        static void Main(string[] args)
        {
            DataHolder dh = new DataHolder();
            DateTime start = DateTime.Now;
            int cnt1 = dh.ReadData();
            LabelSorter ls = new LabelSorter(dh._inputs, dh._target);
            ls.Logic();

            Console.WriteLine("Now running DDR resorter ...");
            Resorter resorter = new Resorter();
            resorter.ReadData(dh._inputs, ls._targetv);
            resorter.Resort(1);
            resorter.Resort(2);
            resorter.Resort(4);
            resorter.Resort(8);
            resorter.Resort(16);

            Console.WriteLine("\nBuilding of sliding window outputs ...");
            SlidingKMEnsemple slider = new SlidingKMEnsemple(dh._inputs, ls._targetv);
            slider.BuildModels(12, 32);

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            double time = duration.Minutes * 60.0 + duration.Seconds + duration.Milliseconds / 1000.0;
            Console.WriteLine("Entire processing time {0:####.00} seconds\n", time);

            Console.WriteLine("Now testing accuracy");
            string fileName = @"..\..\..\..\Test.txt";
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File {0} not found");
                return;
            }

            int counter = 0;
            List<double[]> testInputs = new List<double[]>();
            List<double[]> labelProbs = new List<double[]>();
            string header = string.Empty;
            int[] labelsresorter = null;
            using (StreamReader sr = new StreamReader(fileName))
            {
                header = sr.ReadLine();
                labelsresorter = MakeLabelResorter(ls._sortedLabels, header);
                while (true)
                {
                    string line = sr.ReadLine();
                    if (null == line) break;
                    if (line.Length < 10) break;
                    string[] data = line.Split(",");
                    if (11 != data.Length)
                    {
                        Console.WriteLine("File format {0} violation", fileName);
                        return;
                    }
                    double[] x = new double[5];
                    for (int i = 0; i < x.Length; ++i)
                    {
                        Double.TryParse(data[i], out x[i]);
                    }
                    testInputs.Add(x);
                    double[] y = new double[6];
                    for (int i = 5; i < 11; ++i)
                    {
                        Double.TryParse(data[i], out y[i - 5]);
                    }
                    labelProbs.Add(ResortLabels(labelsresorter, y));
                    ++counter;
                }
            }
            Console.WriteLine("{0} records from test file is read", counter);

            //Test model on unseen inputs
            int cntr = 0;
            List<double> KLs = new List<double>();
            foreach (double[] x in testInputs)
            {
                double[] targets = slider.GetOutput(x);
                double[] probs = GetStat(targets);
                foreach (double d in probs)
                {
                    Console.Write("{0:0.00} ", d);
                }
                Console.WriteLine();
                double[] z = labelProbs[cntr++];
                double KL = KLDiv(z, probs);

                KLs.Add(KL);
            }

            KLs.Sort();
            double avg = 0.0;
            int N = (int)((double)(KLs.Count) * 0.9);
            for (int i = 0; i < N; ++i)
            {
                avg += KLs[i];
            }
            avg /= (double)(N);

            Console.WriteLine("\nAverage for 90% best KLs {0:0.0000}", avg);
        }
    }
}
