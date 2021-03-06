using System;
using System.Collections.Generic;
using System.Drawing;
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
    internal class VideoDataGenerator : AutomaticDataGenerator
    {
        public VideoDataGenerator(string videoFilePath, string outputFolder, Configuration config, ILogger logger) : base(videoFilePath, outputFolder, config, logger) { }

        public override void GenerateData()
        {
            using var video = new VideoCapture(InputFilePath);
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

                // Create data
                CreateAnnotationFiles(sortedDetections, detection, currFrameCount, frameWidth, frameHeight, img);
                
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