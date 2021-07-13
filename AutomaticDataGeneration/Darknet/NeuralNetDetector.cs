using System;
using System.Collections.Generic;
using System.Drawing;
using AutomaticDataGeneration.Darknet.Model;

namespace AutomaticDataGeneration.Darknet
{
    public abstract class NeuralNetDetector
    {
        private readonly List<Detection> _detections;

        protected NeuralNetDetector()
        {
            _detections = new List<Detection>();
        }

        protected abstract void AddDetections(IntPtr image, int size, List<Detection> detections);

        public List<Detection> Detect(IntPtr image, Rectangle region)
        {
            _detections.Clear();
            AddDetections(image, 0, _detections);
            return _detections;
        }

        public unsafe List<Detection> Detect(byte[] image)
        {
            fixed (byte* pnt = image)
            {
                _detections?.Clear();
                AddDetections((IntPtr) pnt, image.Length, _detections);
                return _detections;
            }
        }

        protected void AddDetection(int classId, Rectangle bounds, double confidence)
        {
            _detections.Add(new Detection(bounds, confidence, classId));
        }
    }
}