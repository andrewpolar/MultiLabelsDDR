using System;
using System.Collections.Generic;
using System.Text;

namespace MLabel
{
    class Ensemble
    {
        private List<double[]> _inputs = null;
        private List<double> _target = null;
        private KolmogorovModel[] km = null;

        public Ensemble(List<double[]> inputs, List<double> target)
        {
            _inputs = inputs;
            _target = target;
        }

        public void BuildModels(int NBlocks)
        {
            km = new KolmogorovModel[NBlocks];
            int inputlen = _inputs[0].Length;
            int blocksize = _inputs.Count / NBlocks;
            if (0 != _inputs.Count % NBlocks)
            {
                blocksize += 1;
            }
            List<double[]> blockinput = new List<double[]>();
            List<double> blocktarget = new List<double>();
            int counter = 0;
            int blockIndex = 0;
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
                    km[blockIndex] = new KolmogorovModel(blockinput, blocktarget, new int[] { 3,3,3,3,3 });
                    int NLeaves = 11;
                    int[] linearBlocksPerRootInput = new int[NLeaves];
                    for (int m = 0; m < NLeaves; ++m)
                    {
                        linearBlocksPerRootInput[m] = 12;
                    }
                    km[blockIndex].GenerateInitialOperators(NLeaves, linearBlocksPerRootInput);
                    km[blockIndex].BuildRepresentation(300, 0.01, 0.01);
                    Console.WriteLine("Correlation for estimated and acutal outputs {0:0.00}", km[blockIndex].ComputeCorrelationCoeff());
 
                    blockinput.Clear();
                    blocktarget.Clear();
                    counter = 0;
                    ++blockIndex;

                    //Console.Write("Building ensemble block {0} from {1}  \r", blockIndex, NBlocks);
                }
            }
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
