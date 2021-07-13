using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AutomaticDataGeneration.Darknet.Alturos.Yolo.Config;
using AutomaticDataGeneration.Darknet.Alturos.Yolo.Enums;
using AutomaticDataGeneration.Darknet.Alturos.Yolo.Exceptions;
using AutomaticDataGeneration.Darknet.Alturos.Yolo.Model;

namespace AutomaticDataGeneration.Darknet.Alturos.Yolo
{
    internal class YoloWrapper : IDisposable
    {
        public const int MaxObjects = 1000;
        private const string YoloLibraryCpu = "yolo_cpp_dll_cpu";
        private const string YoloLibraryGpu = "yolo_cpp_dll_gpu";

        private readonly ImageAnalyzer _imageAnalyzer = new();
        private readonly IYoloSystemValidator _yoloSystemValidator;
        private YoloObjectTypeResolver _objectTypeResolver;

        /// <summary>
        ///     Initialize Yolo
        /// </summary>
        /// <param name="yoloConfiguration"></param>
        /// <param name="ignoreGpu">Disable automatic gpu detection</param>
        /// <exception cref="NotSupportedException">Thrown when the process not run in 64bit</exception>
        /// <exception cref="YoloInitializeException">Thrown if an error occurs during initialization</exception>
        public YoloWrapper(YoloConfiguration yoloConfiguration, GpuConfig gpuConfig = null,
            IYoloSystemValidator yoloSystemValidator = null)
        {
            if (yoloSystemValidator == null)
            {
                _yoloSystemValidator = new DefaultYoloSystemValidator();
            }

            Initialize(yoloConfiguration.ConfigFile, yoloConfiguration.WeightsFile, yoloConfiguration.NamesFile,
                gpuConfig);
        }

        /// <summary>
        ///     Initialize Yolo
        /// </summary>
        /// <param name="configurationFilename">Yolo configuration (.cfg) file path</param>
        /// <param name="weightsFilename">Yolo trainded data (.weights) file path</param>
        /// <param name="namesFilename">Yolo object names (.names) file path</param>
        /// <param name="gpu">Gpu Index if multiple graphic devices available</param>
        /// <param name="ignoreGpu">Disable automatic gpu detection</param>
        /// <exception cref="NotSupportedException">Thrown when the process not run in 64bit</exception>
        /// <exception cref="YoloInitializeException">Thrown if an error occurs during initialization</exception>
        public YoloWrapper(string configurationFilename, string weightsFilename, string namesFilename,
            GpuConfig gpuConfig = null, IYoloSystemValidator yoloSystemValidator = null)
        {
            if (yoloSystemValidator == null)
            {
                _yoloSystemValidator = new DefaultYoloSystemValidator();
            }

            Initialize(configurationFilename, weightsFilename, namesFilename, gpuConfig);
        }

        public DetectionSystem DetectionSystem { get; private set; } = DetectionSystem.Unknown;

        public void Dispose()
        {
            switch (DetectionSystem)
            {
                case DetectionSystem.Cpu:
                    DisposeYoloCpu();
                    break;
                case DetectionSystem.Gpu:
                    DisposeYoloGpu();
                    break;
            }
        }

        private void Initialize(string configurationFilename, string weightsFilename, string namesFilename,
            GpuConfig gpuConfig)
        {
            if (IntPtr.Size != 8)
            {
                throw new NotSupportedException("Only 64-bit processes are supported");
            }

            var systemReport = _yoloSystemValidator.Validate();
            if (!systemReport.MicrosoftVisualCPlusPlusRedistributableExists)
            {
                //throw new YoloInitializeException("Microsoft Visual C++ 2017-2019 Redistributable (x64)");
            }

            DetectionSystem = DetectionSystem.Cpu;

            if (gpuConfig != null)
            {
                if (!systemReport.CudaExists)
                {
                    throw new YoloInitializeException("Cuda files not found");
                }

                if (!systemReport.CudnnExists)
                {
                    throw new YoloInitializeException("Cudnn not found");
                }

                var deviceCount = GetDeviceCount();
                if (deviceCount == 0)
                {
                    throw new YoloInitializeException("No Nvidia graphic device is available");
                }

                if (gpuConfig.GpuIndex > deviceCount - 1)
                {
                    throw new YoloInitializeException("Graphic device index is out of range");
                }

                DetectionSystem = DetectionSystem.Gpu;
            }

            switch (DetectionSystem)
            {
                case DetectionSystem.Cpu:
                    InitializeYoloCpu(configurationFilename, weightsFilename, 0);
                    break;
                case DetectionSystem.Gpu:
                    InitializeYoloGpu(configurationFilename, weightsFilename, gpuConfig.GpuIndex);
                    break;
            }

            _objectTypeResolver = new YoloObjectTypeResolver(namesFilename);
        }

        /// <summary>
        ///     Detect objects on an image
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Thrown when the filepath is wrong</exception>
        public IEnumerable<YoloItem> Detect(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("Cannot find the file", filepath);
            }

            var container = new BboxContainer();
            var count = 0;
            switch (DetectionSystem)
            {
                case DetectionSystem.Cpu:
                    count = DetectImageCpu(filepath, ref container);
                    break;
                case DetectionSystem.Gpu:
                    count = DetectImageGpu(filepath, ref container);
                    break;
            }

            if (count == -1)
            {
                throw new NotImplementedException("C++ dll compiled incorrectly");
            }

            return Convert(container);
        }

        /// <summary>
        ///     Detect objects on an image
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Thrown when the yolo_cpp dll is wrong compiled</exception>
        /// <exception cref="Exception">Thrown when the byte array is not a valid image</exception>
        public unsafe IEnumerable<YoloItem> Detect(byte[] imageData, out long time)
        {
            if (!_imageAnalyzer.IsValidImageFormat(imageData))
            {
                throw new Exception("Invalid image data, wrong image format");
            }

            var container = new BboxContainer();

            var count = 0;
            var sw = Stopwatch.StartNew();
            try
            {
                fixed (byte* pnt = imageData)
                {
                    switch (DetectionSystem)
                    {
                        case DetectionSystem.Cpu:
                            count = DetectImageCpu((IntPtr) pnt, imageData.Length, ref container);
                            break;
                        case DetectionSystem.Gpu:
                            count = DetectImageGpu((IntPtr) pnt, imageData.Length, ref container);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                time = sw.ElapsedMilliseconds;
            }

            if (count == -1)
            {
                throw new NotImplementedException("C++ dll compiled incorrectly");
            }

            return Convert(container);
        }

        /// <summary>
        ///     Detect objects on an image
        /// </summary>
        /// <param name="imagePtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Thrown when the yolo_cpp dll is wrong compiled</exception>
        public IEnumerable<YoloItem> Detect(IntPtr imagePtr, int size)
        {
            var container = new BboxContainer();

            var count = 0;
            try
            {
                switch (DetectionSystem)
                {
                    case DetectionSystem.Cpu:
                        count = DetectImageCpu(imagePtr, size, ref container);
                        break;
                    case DetectionSystem.Gpu:
                        count = DetectImageGpu(imagePtr, size, ref container);
                        break;
                }
            }
            catch (Exception)
            {
                return null;
            }

            if (count == -1)
            {
                throw new NotImplementedException("C++ dll compiled incorrectly");
            }

            return Convert(container);
        }

        public string GetGraphicDeviceName(GpuConfig gpuConfig)
        {
            if (gpuConfig == null)
            {
                return string.Empty;
            }

            var systemReport = _yoloSystemValidator.Validate();
            if (!systemReport.CudaExists || !systemReport.CudnnExists)
            {
                return "unknown";
            }

            var deviceName = new StringBuilder(); //allocate memory for string
            GetDeviceName(gpuConfig.GpuIndex, deviceName);
            return deviceName.ToString();
        }

        public bool IsBuiltWithOpenCv()
        {
            return BuiltWithOpenCV();
        }

        private IEnumerable<YoloItem> Convert(BboxContainer container)
        {
            return container.candidates.Where(o => (o.H > 0) || (o.W > 0)).Select(o =>
                new YoloItem
                {
                    X = (int) o.X,
                    Y = (int) o.Y,
                    Height = (int) o.H,
                    Width = (int) o.W,
                    Confidence = o.Prob,
                    Type = _objectTypeResolver.Resolve((int) o.ObjId),
                    TrackId = (int) o.TrackId
                }
            );
        }

        #region DllImport Cpu

        [DllImport(YoloLibraryCpu, EntryPoint = "init")]
        private static extern int InitializeYoloCpu(string configurationFilename, string weightsFilename, int gpuIndex);

        [DllImport(YoloLibraryCpu, EntryPoint = "detect_image")]
        internal static extern int DetectImageCpu(string filename, ref BboxContainer container);

        [DllImport(YoloLibraryCpu, EntryPoint = "detect_mat")]
        internal static extern int DetectImageCpu(IntPtr pArray, int nSize, ref BboxContainer container);

        [DllImport(YoloLibraryCpu, EntryPoint = "dispose")]
        internal static extern int DisposeYoloCpu();

        [DllImport(YoloLibraryCpu, EntryPoint = "built_with_opencv")]
        internal static extern bool BuiltWithOpenCV();

        #endregion

        #region DllImport Gpu

        [DllImport(YoloLibraryGpu, EntryPoint = "init")]
        internal static extern int InitializeYoloGpu(string configurationFilename, string weightsFilename, int gpuIndex);

        [DllImport(YoloLibraryGpu, EntryPoint = "detect_image")]
        internal static extern int DetectImageGpu(string filename, ref BboxContainer container);

        [DllImport(YoloLibraryGpu, EntryPoint = "detect_mat")]
        internal static extern int DetectImageGpu(IntPtr pArray, int nSize, ref BboxContainer container);

        [DllImport(YoloLibraryGpu, EntryPoint = "dispose")]
        internal static extern int DisposeYoloGpu();

        [DllImport(YoloLibraryGpu, EntryPoint = "get_device_count")]
        internal static extern int GetDeviceCount();

        [DllImport(YoloLibraryGpu, EntryPoint = "get_device_name")]
        internal static extern int GetDeviceName(int gpu, StringBuilder deviceName);

        #endregion
    }
}