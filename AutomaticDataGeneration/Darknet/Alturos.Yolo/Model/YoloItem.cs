#region

using System.Drawing;

#endregion

namespace AutomaticDataGeneration.Darknet.Alturos.Yolo.Model
{
    public class YoloItem
    {
        public string Type { get; set; }
        public double Confidence { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TrackId { get; set; }

        public Point Center()
        {
            return new(X + (Width / 2), Y + (Height / 2));
        }
    }
}