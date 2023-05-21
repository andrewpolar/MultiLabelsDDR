using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MLabel
{
    class Resorter
    {
        public static List<double[]> _inputs = null;
        public static List<double> _target = null;

        public bool ReadData(List<double[]> inputs, List<double> target)
        {
            _inputs = inputs;
            _target = target;

            return true;
        }

        public void Resort(int NBlocks)
        {
            int inputlen = _inputs[0].Length;
            int blocksize = _inputs.Count / NBlocks;
            if (0 != _inputs.Count % NBlocks)
            {
                blocksize += 1;
            }
            List<double[]> blockinput = new List<double[]>();
            List<double> blocktarget = new List<double>();
            int counter = 0;
            List<double[]> sortedinputs = new List<double[]>();
            List<double> sortedtarget = new List<double>();
            for (int j = 0; j < _inputs.Count; ++j)
            {
                double[] x = new double[inputlen];
                for (int k = 0; k < inputlen; ++k)
                {
                    x[k] = _inputs[j][k];
                }
                double t = _target[j];

                blockinput.Add(x);
                blocktarget.Add(t);

                if (++counter >= blocksize || j >= _inputs.Count - 1)
                {
                    KolmogorovModel km = new KolmogorovModel(blockinput, blocktarget, new int[] { 3, 3, 3, 3, 3 });
                    int NLeaves = 11;
                    int[] linearBlocksPerRootInput = new int[NLeaves];
                    for (int m = 0; m < NLeaves; ++m)
                    {
                        linearBlocksPerRootInput[m] = 12;
                    }
                    km.GenerateInitialOperators(NLeaves, linearBlocksPerRootInput);
                    km.BuildRepresentation(500, 0.01, 0.01);
                    Console.WriteLine("Correlation for estimated and actual outputs {0:0.00}", km.ComputeCorrelationCoeff());
                    km.SortData();

                    for (int k = 0; k < km._inputs.Count; ++k)
                    {
                        double[] z = new double[inputlen];
                        for (int m = 0; m < inputlen; ++m)
                        {
                            z[m] = km._inputs[k][m];
                        }
                        double t2 = km._target[k];

                        sortedinputs.Add(z);
                        sortedtarget.Add(t2);
                    }

                    blockinput.Clear();
                    blocktarget.Clear();
                    counter = 0;
                }
            }

            _inputs.Clear();
            _target.Clear();

            for (int k = 0; k < sortedinputs.Count; ++k)
            {
                double[] z = new double[inputlen];
                for (int m = 0; m < inputlen; ++m)
                {
                    z[m] = sortedinputs[k][m];
                }
                double t2 = sortedtarget[k];

                _inputs.Add(z);
                _target.Add(t2);
            }
            //Console.WriteLine();
        }
    }
}
