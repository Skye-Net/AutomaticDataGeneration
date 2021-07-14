using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AutomaticDataGeneration.Config;
using AutomaticDataGeneration.Darknet;
using AutomaticDataGeneration.Darknet.Model;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging;

namespace AutomaticDataGeneration.DataGenerators
{
    public abstract class AutomaticDataGenerator
    {
        protected YoloDarknet _detector;
        protected string InputFilePath { get; }
        protected string OutputFolder { get; }
        protected Configuration Configuration { get; }
        protected ILogger Logger { get; }

        public abstract void GenerateData();

        protected AutomaticDataGenerator(string inputFolder, string outputFolder, Configuration config, ILogger logger)
        {
            InputFilePath = inputFolder;
            OutputFolder = outputFolder;
            Logger = logger;
            Configuration = config;
            _detector = new YoloDarknet(config.DarknetConfig.ConfigPath, config.DarknetConfig.WeightPath, config.DarknetConfig.NamesPath);
        }
        
        protected void RenderPreview(Mat img, List<Detection> detections, double frameHeight, double frameWidth, int totalFrames, int currentFrame)
        {
            if (Configuration.PreviewConfig.ShowImageMetadata)
            {
                CvInvoke.PutText(img, $"{currentFrame}/{totalFrames}", new Point(0, 0), FontFace.HersheyPlain, 12f, new MCvScalar(255,0,0));
                CvInvoke.PutText(img, $"{Math.Floor(frameWidth)}*{Math.Floor(frameHeight)}", new Point((int) (frameWidth * 0.90), 0), FontFace.HersheyPlain, 12f, new MCvScalar(255,0,0));
            }
            foreach (var detection in detections)
            {
                if (Configuration.PreviewConfig.ShowConfidence)
                {
                    CvInvoke.PutText(img, Math.Abs(detection.Confidence * 100).ToString(), new Point(detection.BoundingBox.Left, detection.BoundingBox.Top), FontFace.HersheyPlain, 12f, new MCvScalar(255,0,0));
                }

                if (Configuration.PreviewConfig.ShowAnnotations)
                {
                    CvInvoke.Rectangle(img, detection.BoundingBox, new MCvScalar(255,0,0));
                }
            }
            CvInvoke.Imshow("Current data", img);
            CvInvoke.WaitKey(1);
        }

        protected void CreateAnnotationFiles(List<Detection> sortedDetections, List<Detection> detection, int currFrameCount, double frameWidth, double frameHeight, Mat img)
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
                    img.Save(Path.Combine(OutputFolder ?? InputFilePath, baseFileName));
                    File.WriteAllText(Path.Combine(OutputFolder ?? InputFilePath, textName), fileText);
                }
            }
        }
    }
}