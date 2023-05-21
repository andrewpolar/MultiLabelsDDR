using System;
using System.Collections.Generic;
using System.Text;

namespace MLabel
{
    class SlidingKMEnsemple
    {
        private List<double[]> _inputs = null;
        private List<double> _target = null;
        private KolmogorovModel[] km = null;

        public SlidingKMEnsemple(List<double[]> inputs, List<double> target)
        {
            _inputs = inputs;
            _target = target;
        }

        public void BuildModels(int SortedNBlocks, int nWantedModels)
        {
            int blockLength = _inputs.Count / SortedNBlocks;
            int shiftSize = (_inputs.Count - blockLength) / (nWantedModels - 1);
            List<int> A = new List<int>();
            List<int> B = new List<int>();
            int currentA = 0;
            int currentB = blockLength - 1;
            A.Add(currentA);
            B.Add(currentB);
            for (int i = 1; i < nWantedModels; ++i)
            {
                currentA += shiftSize;
                currentB += shiftSize;
                A.Add(currentA);
                if (_inputs.Count - 1 - currentB < shiftSize)
                {
                    currentB = _inputs.Count - 1;
                }
                B.Add(currentB);
                if (currentB >= _inputs.Count - 1)
                {
                    break;
                }
            }

            int models = A.Count;
            km = new KolmogorovModel[models];
            List<double[]> x = new List<double[]>();
            List<double> y = new List<double>();
            int nxsize = _inputs[0].Length;
            for (int i = 0; i < models; ++i)
            {
                x.Clear();
                y.Clear();
                for (int k = A[i]; k <= B[i]; ++k)
                {
                    double[] currentx = new double[nxsize];
                    for (int j = 0; j < nxsize; ++j)
                    {
                        currentx[j] = _inputs[k][j];
                    }
                    x.Add(currentx);
                    y.Add(_target[k]);
                }

                km[i] = new KolmogorovModel(x, y, new int[] { 3, 3, 3, 3, 3 });
                int NLeaves = 4;
                int[] linearBlocksPerRootInput = new int[NLeaves];
                for (int m = 0; m < NLeaves; ++m)
                {
                    linearBlocksPerRootInput[m] = 12;
                }
                km[i].GenerateInitialOperators(NLeaves, linearBlocksPerRootInput);
                km[i].BuildRepresentation(500, 0.01, 0.01);
                Console.WriteLine("Correlation for estimated and acutal outputs {0:0.00}", km[i].ComputeCorrelationCoeff());
            }
            Console.WriteLine();
        }

        public double[] GetOutput(double[] x)
        {
            double[] y = new double[km.Length];
            for (int i = 0; i < km.Length; ++i)
            {
                y[i] = km[i].ComputeOutput(x);
            }

            return y;
        }
    }
}