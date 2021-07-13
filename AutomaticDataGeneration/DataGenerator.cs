using System;
using System.IO;
using AutomaticDataGeneration.Config;
using AutomaticDataGeneration.DataGenerators;
using Microsoft.Extensions.Logging;

namespace AutomaticDataGeneration
{
    public class DataGenerator
    {
        public DataGenerator(Configuration config)
        {
            Config = config;
        }

        public Configuration Config { get; }

        /// <summary>
        /// Generates yolo data based on a video
        /// </summary>
        /// <param name="videoFilePath">Path to the video</param>
        /// <param name="outputFolder">Output folder for the yolo data</param>
        /// <param name="logger">An optional logger if verbose logging is enabled</param>
        /// <exception cref="FileNotFoundException">If there is no video file</exception>
        /// <exception cref="ArgumentNullException">If the configuration dictates that verbose logging is enabled but the logger is null</exception>
        public void GenerateDataFromVideo(string videoFilePath, string outputFolder, ILogger logger = null)
        {
            if (!File.Exists(videoFilePath))
            {
                throw new FileNotFoundException("No such file exists");
            }

            var outputDirectory = new DirectoryInfo(outputFolder);
            if (!outputDirectory.Exists)
            {
                outputDirectory.Create();
            }

            if (Config.EnableVerboseLogging && logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            var videoDataGenerator = new VideoDataGenerator(videoFilePath, outputFolder, Config, logger);
            videoDataGenerator.GenerateData();
        }

        public void AnnotateImagesInFolder(string inputFolder, string outputFolder = null)
        {
        }
    }
}