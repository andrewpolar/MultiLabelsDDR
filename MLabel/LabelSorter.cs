using System;
using System.Collections.Generic;
using System.Text;

namespace MLabel
{
    class LabelSorter
    {
        private List<double[]> _inputs = null;
        private List<string> _target = null;
        public List<double> _targetv = null;

        Queue<string> _allLabels = new Queue<string>();
        public List<string> _sortedLabels = new List<string>();
 
        public LabelSorter(List<double[]> inputs, List<string> target)
        {
            _inputs = inputs;
            _target = target;
            _allLabels.Enqueue("S");
            _allLabels.Enqueue("Q");
            _allLabels.Enqueue("N");
            _allLabels.Enqueue("R");
            _allLabels.Enqueue("B");
            _allLabels.Enqueue("F");
        }

        private double GetIndex(List<string> list, string label)
        {
            double index = 1.0;
            bool isFound = false;
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].Trim().Contains(label.Trim()))
                {
                    isFound = true;
                    break;
                }
                index += 1.0;
            }
            if (isFound) return index;
            else return -1.0;
        }

        private double GetModelResidual(List<double[]> inputs, List<double> targets)
        {
            KolmogorovModel km = new KolmogorovModel(inputs, targets, new int[] { 3, 3, 3, 3, 3 });
            int NLeaves = 2;
            int[] linearBlocksPerRootInput = new int[NLeaves];
            for (int m = 0; m < NLeaves; ++m)
            {
                linearBlocksPerRootInput[m] = 6;
            }
            km.GenerateInitialOperators(NLeaves, linearBlocksPerRootInput);
            km.BuildRepresentation(200, 0.01, 0.01);
            double coeff = km.ComputeCorrelationCoeff();            
            return coeff;
        }

        private List<double[]> GetInputs(List<string> labels)
        {
            List<double[]> inputs = new List<double[]>();
            for (int i = 0; i < _inputs.Count; ++i)
            {
                double index = GetIndex(labels, _target[i]);
                if (index > 0.0)
                {
                    double[] x = new double[5];
                    double[] z = _inputs[i];
                    for (int j = 0; j < 5; ++j)
                    {
                        x[j] = z[j];
                    }
                    inputs.Add(x);
                }
            }
            return inputs;
        }

        private List<double> GetTargets(List<string> labels)
        {
            List<double> targets = new List<double>();
            for (int i = 0; i < _target.Count; ++i)
            {
                double index = GetIndex(labels, _target[i]);
                if (index > 0.0)
                {
                    targets.Add(index);
                }
            }
            return targets;
        }

        private List<string> CopyAndReplace(List<string> labels, int prev, int current)
        {
            List<string> list = new List<string>();
            foreach (string s in labels)
            {
                list.Add(s);
            }
            string tmp = list[current];
            list[current] = list[prev];
            list[prev] = tmp;
            return list;
        }

        public void Logic()
        {
            Console.WriteLine("Sorting labels...");
            List<string> list = new List<string>();
            for (int i = 0; i < 3; ++i)
            {
                list.Add(_allLabels.Dequeue());
            }
            list = Logic3(list);
            Console.WriteLine(list[0] + list[1] + list[2]);

            list.Add(_allLabels.Dequeue());
            list = Logic4(list);
            Console.WriteLine(list[0] + list[1] + list[2] + list[3]);

            list.Add(_allLabels.Dequeue());
            list = Logic5(list);
            Console.WriteLine(list[0] + list[1] + list[2] + list[3] + list[4]);

            list.Add(_allLabels.Dequeue());
            list = Logic6(list);
            Console.WriteLine(list[0] + list[1] + list[2] + list[3] + list[4] + list[5]);
            Console.WriteLine("Completed");

            foreach (string s in list)
            {
                _sortedLabels.Add(s);
            }

            _targetv = new List<double>();
            for (int i = 0; i < _inputs.Count; ++i)
            {
                double index = GetIndex(_sortedLabels, _target[i]);
                if (index > 0.0)
                {                 
                    _targetv.Add(index);
                }
            }
        }

        private List<string> Logic6(List<string> list)
        {
            List<double[]> inputs = GetInputs(list);
            List<double> targets = GetTargets(list);
            double residual1 = GetModelResidual(inputs, targets);

            List<string> list1 = CopyAndReplace(list, 4, 5);
            targets.Clear();
            targets = GetTargets(list1);
            double residual2 = GetModelResidual(inputs, targets);
 
            List<string> list2 = CopyAndReplace(list1, 3, 4);
            targets.Clear();
            targets = GetTargets(list2);
            double residual3 = GetModelResidual(inputs, targets);

            List<string> list3 = CopyAndReplace(list2, 2, 3);
            targets.Clear();
            targets = GetTargets(list3);
            double residual4 = GetModelResidual(inputs, targets);

            List<string> list4 = CopyAndReplace(list3, 1, 2);
            targets.Clear();
            targets = GetTargets(list4);
            double residual5 = GetModelResidual(inputs, targets);
 
            List<string> list5 = CopyAndReplace(list4, 0, 1);
            targets.Clear();
            targets = GetTargets(list5);
            double residual6 = GetModelResidual(inputs, targets);
 
            if (residual1 >= residual2 && residual1 >= residual3 && residual1 >= residual4 && residual1 >= residual5 && residual1 >= residual6)
            {
                return list;
            }
            if (residual2 >= residual1 && residual2 >= residual3 && residual2 >= residual4 && residual2 >= residual5 && residual2 >= residual6)
            {
                return list1;
            }
            if (residual3 >= residual1 && residual3 >= residual2 && residual3 >= residual4 && residual3 >= residual5 && residual3 >= residual6)
            {
                return list2;
            }
            if (residual4 >= residual1 && residual4 >= residual2 && residual4 >= residual3 && residual4 >= residual5 && residual4 >= residual6)
            {
                return list3;
            }
            if (residual5 >= residual1 && residual5 >= residual2 && residual5 >= residual3 && residual5 >= residual4 && residual5 >= residual6)
            {
                return list4;
            }
            if (residual6 >= residual1 && residual6 >= residual2 && residual6 >= residual3 && residual6 >= residual4 && residual6 >= residual5)
            {
                return list5;
            }

            return null;
        }

        private List<string> Logic5(List<string> list)
        {
            List<double[]> inputs = GetInputs(list);
            List<double> targets = GetTargets(list);
            double residual1 = GetModelResidual(inputs, targets);

            List<string> list1 = CopyAndReplace(list, 3, 4);
            targets.Clear();
            targets = GetTargets(list1);
            double residual2 = GetModelResidual(inputs, targets);

            List<string> list2 = CopyAndReplace(list1, 2, 3);
            targets.Clear();
            targets = GetTargets(list2);
            double residual3 = GetModelResidual(inputs, targets);

            List<string> list3 = CopyAndReplace(list2, 1, 2);
            targets.Clear();
            targets = GetTargets(list3);
            double residual4 = GetModelResidual(inputs, targets);
 
            List<string> list4 = CopyAndReplace(list3, 0, 1);
            targets.Clear();
            targets = GetTargets(list4);
            double residual5 = GetModelResidual(inputs, targets);
  
            if (residual1 >= residual2 && residual1 >= residual3 && residual1 >= residual4 && residual1 >= residual5)
            {
                return list;
            }
            if (residual2 >= residual1 && residual2 >= residual3 && residual2 >= residual4 && residual2 >= residual5)
            {
                return list1;
            }
            if (residual3 >= residual1 && residual3 >= residual2 && residual3 >= residual4 && residual3 >= residual5) 
            { 
                return list2;
            }
            if (residual4 >= residual1 && residual4 >= residual2 && residual4 >= residual3 && residual4 >= residual5)
            {
                return list3;
            }
            if (residual5 >= residual1 && residual5 >= residual2 && residual5 >= residual3 && residual5 >= residual4)
            {
                return list4;
            }
            return null;
        }

        private List<string> Logic4(List<string> list)
        {
            List<double[]> inputs = GetInputs(list);
            List<double> targets = GetTargets(list);
            double residual1 = GetModelResidual(inputs, targets);

            List<string> list1 = CopyAndReplace(list, 2, 3);
            targets.Clear();
            targets = GetTargets(list1);
            double residual2 = GetModelResidual(inputs, targets);
  
            List<string> list2 = CopyAndReplace(list1, 1, 2);
            targets.Clear();
            targets = GetTargets(list2);
            double residual3 = GetModelResidual(inputs, targets);

            List<string> list3 = CopyAndReplace(list2, 0, 1);
            targets.Clear();
            targets = GetTargets(list3);
            double residual4 = GetModelResidual(inputs, targets);
 
            if (residual1 >= residual2 && residual1 >= residual3 && residual1 >= residual4)
            {
                return list;
            }
            if (residual2 >= residual1 && residual2 >= residual3 && residual2 >= residual4)
            {
                return list1;
            }
            if (residual3 >= residual1 && residual3 >= residual2 && residual3 >= residual4)
            {
                return list2;
            }
            if (residual4 >= residual1 && residual4 >= residual2 && residual4 >= residual3)
            {
                return list3;
            }
            return null;
        }

        public List<string> Logic3(List<string> list)
        {
            List<double[]> inputs = GetInputs(list);
            List<double> targets = GetTargets(list);
            double residual1 = GetModelResidual(inputs, targets);

            List<string> list1 = CopyAndReplace(list, 1, 2);
            targets.Clear();
            targets = GetTargets(list1);
            double residual2 = GetModelResidual(inputs, targets);

            List<string> list2 = CopyAndReplace(list1, 0, 1);
            targets.Clear();
            targets = GetTargets(list2);
            double residual3 = GetModelResidual(inputs, targets);

            if (residual1 >= residual2 && residual1 >= residual2)
            {
                return list;
            }
            if (residual2 >= residual1 && residual2 >= residual3)
            {
                return list1;
            }
            if (residual3 >= residual1 && residual3 >= residual2)
            {
                return list2;
            }
            return null;
        }
    }
}
