using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiiGestureLib
{
    [Serializable]
    public class Gesture
    {
        public string Name { get; set; }
        public int Count { get { return data.Count; } }

        public Gesture()
        {
            Name = "<NoName>";
            data = new List<double[]>();
        }

        public IEnumerable<double[]> GetSamples()
        {
            return data;
        }

        public void AddSample(double[] sample)
        {
            data.Add(sample);
        }

        public double DistanceTo(Gesture other)
        {
            double[] current = new double[other.Count];
            double[] previous = new double[other.Count];

            current[0] = 2 * localDist(data[0], other.data[0]);
            for (int j = 1; j < other.Count; j++) 
                current[j] = current[j - 1] + localDist(data[0], other.data[j]);
            for (int i = 1; i < data.Count; i++)
            {
                { double[] aux = current; current = previous; previous = aux; }
                current[0] = previous[0] + localDist(data[i], other.data[0]);
                for (int j = 1; j < other.data.Count; j++)
                {
                    double d = localDist(data[i], other.data[j]);
                    current[j] = Math.Min(Math.Min(current[j - 1], previous[j]), previous[j - 1] + d) + d;
                }
            }
            return current[other.data.Count - 1] / (data.Count + other.data.Count);
        }

        #region private

        List<double[]> data;

        double localDist(double[] a, double[] b)
        {
            return Math.Sqrt((a[0] - b[0]) * (a[0] - b[0]) + 
                             (a[1] - b[1]) * (a[1] - b[1]) + 
                             (a[2] - b[2]) * (a[2] - b[2]));
        }

        #endregion
    }
}
