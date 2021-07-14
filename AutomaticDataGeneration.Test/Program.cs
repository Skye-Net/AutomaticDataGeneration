using System;
using System.IO;
using AutomaticDataGeneration.Config;

namespace AutomaticDataGeneration.Test
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = Path.Combine(basePath, "DarknetFiles", "yolov2-tiny-voc.cfg");
            var weightPath = Path.Combine(basePath, "DarknetFiles", "yolov2-tiny-voc.weights");
            var namesPath = Path.Combine(basePath, "DarknetFiles", "voc.names");
            var images = Path.Combine(basePath, "TestImages");

            var config = new Configuration(false, true, 80, configPath, weightPath, namesPath, true, true, true);
            var dataGen = new DataGenerator(config);
            dataGen.AnnotateImagesInFolder(images);
        }
    }
}