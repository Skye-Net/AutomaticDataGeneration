using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutomaticDataGeneration.Config;
using AutomaticDataGeneration.Darknet;
using AutomaticDataGeneration.Darknet.Model;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace AutomaticDataGeneration.DataGenerators
{
    internal class VideoDataGenerator
    {
        private readonly YoloDarknet _detector;

        internal VideoDataGenerator(string videoFilePath, string outputFolder, Configuration config, ILogger logger)
        {
            VideoFilePath = videoFilePath;
            OutputFolder = outputFolder;
            Logger = logger;
            Configuration = config;
            _detector = new YoloDarknet(config.DarknetConfig.ConfigPath, config.DarknetConfig.WeightPath, config.DarknetConfig.NamesPath);
        }

        private string VideoFilePath { get; }
        private string OutputFolder { get; }
        private Configuration Configuration { get; }
        private ILogger Logger { get; }

        public void GenerateData()
        {
            using var video = new VideoCapture(VideoFilePath);
            using var img = new Mat();

            // Extract video metadata
            var totalFrames = (int) Math.Floor(video.Get(CapProp.FrameCount));
            var frameHeight = video.Get(CapProp.FrameHeight);
            var frameWidth = video.Get(CapProp.FrameWidth);

            var currFrameCount = 1;

            while (video.Grab())
            {
                // Get data from video buffer
                video.Retrieve(img);

                // Transform into valid data for Yolo
                var imgData = img.ToImage<Bgr, byte>().ToJpegData(100);

                // Fetch detections
                var detection = _detector.Detect(imgData);
                var sortedDetections = detection.Where(x => x.Confidence >= Configuration.MinimumConfidence).ToList();

                // Render preview for user
                if (Configuration.PreviewConfig.EnablePreview)
                {
                    RenderPreview(img, detection, frameHeight, frameWidth, totalFrames, currFrameCount);
                }

                if (Configuration.EnableVerboseLogging)
                {
                    Logger.LogInformation($"Frame {currFrameCount}/{totalFrames} had {detection.Count} detections where {sortedDetections.Count} were within confidence");
                }

                // Create data
                CreateAnnotationFiles(sortedDetections, detection, currFrameCount, frameWidth, frameHeight, img);
                currFrameCount += 1;
            }
        }

        private void RenderPreview(Mat img, List<Detection> detection, double frameHeight, double frameWidth, int totalFrames, int currentFrame)
        {
            CvInvoke.Imshow("Current data", img);
            CvInvoke.WaitKey(1);
        }

        private void CreateAnnotationFiles(List<Detection> sortedDetections, List<Detection> detection, int currFrameCount, double frameWidth, double frameHeight, Mat img)
        {
            if ((sortedDetections.Count > 0) || (Configuration.SaveInvalidData && (detection.Count == 0)))
            {
                // Generate unique name based on current tick count and frame count
                var filenameTick = DateTime.Now.Ticks;
                var baseFileName = $"{currFrameCount}_{filenameTick}.png";

                // Generate yolo text file data
                var fileText = string.Empty;
                foreach (var detect in sortedDetections)
                {
                    fileText += string.IsNullOrEmpty(fileText)
                        ? detect.GetYoloString(frameWidth, frameHeight)
                        : Environment.NewLine + detect.GetYoloString(frameWidth, frameHeight);
                }

                if (!string.IsNullOrWhiteSpace(fileText) || (Configuration.SaveInvalidData && (detection.Count == 0)))
                {
                    var textName = $"{currFrameCount}_{filenameTick}.txt";
                    img.Save(Path.Combine(OutputFolder, baseFileName));
                    File.WriteAllText(Path.Combine(OutputFolder, textName), fileText);
                }
            }
        }
    }
}