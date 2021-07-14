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
        ///     Generates yolo data based on a video
        /// </summary>
        /// <param name="videoFilePath">Path to the video</param>
        /// <param name="outputFolder">Output folder for the yolo data</param>
        /// <param name="logger">An optional logger if verbose logging is enabled</param>
        /// <exception cref="FileNotFoundException">If there is no video file</exception>
        /// <exception cref="ArgumentNullException">
        ///     If the configuration dictates that verbose logging is enabled but the logger is
        ///     null
        /// </exception>
        public void GenerateDataFromVideo(string videoFilePath, string outputFolder, ILogger logger = null)
        {
            if (!File.Exists(videoFilePath))
            {
                throw new FileNotFoundException("No such file exists");
            }

            if (Config is null)
            {
                throw new ArgumentNullException(nameof(Config));
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

        /// <summary>
        ///     Generates yolo data based on a folder of images
        /// </summary>
        /// <param name="inputFolder">Path to the folder with images</param>
        /// <param name="outputFolder">Optional output directory</param>
        /// <param name="logger">An optional logger if verbose logging is enabled</param>
        /// <exception cref="ArgumentException">If invalid arguments are passed</exception>
        /// <exception cref="ArgumentNullException">
        ///     If the configuration dictates that verbose logging is enabled but the logger is
        ///     null
        /// </exception>
        public void AnnotateImagesInFolder(string inputFolder, string outputFolder = null, ILogger logger = null)
        {
            var directoryInfo = new DirectoryInfo(inputFolder);
            if (!directoryInfo.Exists)
            {
                throw new ArgumentException(nameof(inputFolder));
            }

            if (Config is null)
            {
                throw new ArgumentException(nameof(Config));
            }

            if (Config.EnableVerboseLogging && logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (outputFolder is null && Config.EnableVerboseLogging)
            {
                logger.LogInformation("Output directory is null, so we're saving data into the input directory");
            }

            if (!string.IsNullOrWhiteSpace(outputFolder))
            {
                var outputDirectory = new DirectoryInfo(outputFolder);
                if (!outputDirectory.Exists)
                {
                    outputDirectory.Create();
                }
            }

            var imageDataGenerator = new ImageDataGenerator(inputFolder, outputFolder, Config, logger);
            imageDataGenerator.GenerateData();
        }
    }
}