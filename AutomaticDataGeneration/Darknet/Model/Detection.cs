using System.Drawing;

namespace AutomaticDataGeneration.Darknet.Model
{
    public readonly struct Detection
    {
        public readonly Rectangle BoundingBox;
        public readonly double Confidence; // In the interval [0.0, 1.0].
        public readonly int ClassId;

        public Detection(Rectangle boundingBox, double confidence, int classId)
        {
            BoundingBox = boundingBox;
            Confidence = confidence;
            ClassId = classId;
        }

        public bool IsEmpty => BoundingBox.IsEmpty;

        public string GetYoloString(double inputWidth, double inputHeight)
        {
            var relativeCenterX = (BoundingBox.X + (BoundingBox.Width / 2f)) / inputWidth;
            var relativeCenterY = (BoundingBox.Y + (BoundingBox.Height / 2f)) / inputHeight;
            var relativeWidth = BoundingBox.Width / inputWidth;
            var relativeHeight = BoundingBox.Height / inputHeight;

            return $"{ClassId} {relativeCenterX} {relativeCenterY} {relativeWidth} {relativeHeight}".Replace(",", ".");
        }
    }
}