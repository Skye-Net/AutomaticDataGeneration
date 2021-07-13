using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using AutomaticDataGeneration.Darknet.Alturos.Yolo;
using AutomaticDataGeneration.Darknet.Alturos.Yolo.Config;
using AutomaticDataGeneration.Darknet.Model;

namespace AutomaticDataGeneration.Darknet
{
    internal class YoloDarknet : NeuralNetDetector, IDarknet
    {
        private readonly YoloWrapper _darknet;
        private readonly GpuConfig _gpuConfig;

        public YoloDarknet(string configPath, string weightPath, string namesPath)
        {
            _gpuConfig ??= new GpuConfig
            {
                GpuIndex = 0
            };

            if (!File.Exists(configPath) || !File.Exists(weightPath) || !File.Exists(namesPath))
            {
                throw new FileNotFoundException("Not all valid yolo config files found");
            }

            _darknet = new YoloWrapper(configPath, weightPath, namesPath, _gpuConfig);
        }

        public void Dispose()
        {
            _darknet?.Dispose();
        }


        internal void AddDetections(IntPtr image, int size)
        {
            if ((_darknet == null) || (image == IntPtr.Zero))
            {
                return;
            }

            var results = _darknet.Detect(image, size).ToList();

            if (results.Count <= 0)
            {
                return;
            }

            foreach (var result in results)
            {
                var bounds = new Rectangle(result.X, result.Y, result.Width, result.Height);
                AddDetection(result.TrackId, bounds, result.Confidence);
            }
        }

        protected override void AddDetections(IntPtr image, int size, List<Detection> detections)
        {
            if ((_darknet == null) || (image == IntPtr.Zero))
            {
                throw new ArgumentNullException(nameof(image));
            }

            var results = _darknet.Detect(image, size).ToList();

            if (results.Count <= 0)
            {
                return;
            }

            foreach (var result in results)
            {
                var bounds = new Rectangle(result.X, result.Y, result.Width, result.Height);
                AddDetection(result.TrackId, bounds, result.Confidence);
            }
        }
    }
}