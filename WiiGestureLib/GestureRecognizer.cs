using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiiGestureLib
{
    public delegate void GestureRecognizedEventHandler(string gestureName);

    public class GestureRecognizer
    {
        public event GestureRecognizedEventHandler GestureRecognized;
        public List<Gesture> Prototypes { get; private set; }

        public GestureRecognizer()
        {
            Prototypes = new List<Gesture>();
        }

        public void Clear()
        {
            Prototypes.Clear();
        }

        public void AddPrototype(Gesture gesture)
        {
            Prototypes.Add(gesture);
        }

        public void OnGestureCaptured(Gesture gesture)
        {
            if (Prototypes.Count == 0) return;

            double mindist = double.MaxValue;
            Gesture bestGesture = null;

            foreach (var g in Prototypes)
            {
                double distance = gesture.DistanceTo(g);
                if (distance < mindist)
                {
                    mindist = distance;
                    bestGesture = g;
                }
            }

            if (GestureRecognized != null)
                GestureRecognized(bestGesture.Name);
        }
    }
}
