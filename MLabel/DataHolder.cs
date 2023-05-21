using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MLabel
{
    class DataHolder
    {
        public List<double[]> _inputs = new List<double[]>();
        public List<string> _target = new List<string>();

        public int ReadData()
        {
            _inputs.Clear();
            _target.Clear();
            string fileName = @"..\..\..\..\Training.txt";
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File {0} not found");
                return -1;
            }

            int cnt = 0;
            using (StreamReader sr = new StreamReader(fileName))
            {
                while (true)
                {
                    string line = sr.ReadLine();
                    if (null == line) break;
                    if (line.Length < 10) break;
                    string[] data = line.Split(",");
                    if (6 != data.Length) return -1;
                    double[] x = new double[5];
                    for (int i = 0; i < x.Length; ++i)
                    {
                        Double.TryParse(data[i], out x[i]);
                    }
                    _inputs.Add(x);
                    _target.Add(data[5].Trim());
                    ++cnt;
                }
            }
            return cnt;
        }
    }
 }
