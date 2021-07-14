using System.Linq;
using AutomaticDataGeneration.Config;
using AutomaticDataGeneration.Darknet;
using AutomaticDataGeneration.Extensions;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace AutomaticDataGeneration.DataGenerators
{
    public class ImageDataGenerator : AutomaticDataGenerator
    {
        private readonly string[] _extensions = new[] {"jpg","jpeg", "png", "bmp"};
        public ImageDataGenerator(string inputFolder, string outputFolder, Configuration config, ILogger logger) : base(inputFolder, outputFolder, config, logger) { }

        public override void GenerateData()
        {
            var files = FileExtensions.FilterFiles(InputFilePath, _extensions).ToList();
            var totalFrames = files.Count();
            var currFrameCount = 0;
            foreach (var imagePath in files)
            {
                using var img = new Mat(imagePath);
                var frameHeight = img.Height;
                var frameWidth = img.Width;
                // Transform into valid data for Yolo
                var imgData = img.ToImage<Bgr, byte>().ToJpegData(100);

                // Fetch detections
                var detection = _detector.Detect(imgData);
                var sortedDetections = detection.Where(x => x.Confidence >= Configuration.MinimumConfidence).ToList();

                // Create data
                CreateAnnotationFiles(sortedDetections, detection, currFrameCount, frameWidth, frameHeight, img, imagePath);

                
                // Render preview for user
                if (Configuration.PreviewConfig.EnablePreview)
                {
                    RenderPreview(img, detection, frameHeight, frameWidth, totalFrames, currFrameCount);
                }

                if (Configuration.EnableVerboseLogging)
                {
                    Logger.LogInformation($"Frame {currFrameCount}/{totalFrames} had {detection.Count} detections where {sortedDetections.Count} were within confidence");
                }
                
                currFrameCount += 1;
            }

        }
    }
}