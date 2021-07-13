using System;

namespace AutomaticDataGeneration.Config
{
    public class Configuration
    {
        public DarknetConfig DarknetConfig { get; set; }
        public PreviewConfig PreviewConfig { get; set; }
        public bool EnableVerboseLogging { get; set; }
        public bool SaveInvalidData { get; set; }
        public double MinimumConfidence { get; set; }
        
        public Configuration(bool enableVerboseLogging, bool saveInvalidData, double minimumConfidence, string configPath, string weightPath, string namesPath, bool enablePreview, bool showAnnotations, bool showConfidence)
        {
            EnableVerboseLogging = enableVerboseLogging;
            SaveInvalidData = saveInvalidData;
            MinimumConfidence = minimumConfidence;
            DarknetConfig = new DarknetConfig(configPath, weightPath, namesPath);
            PreviewConfig = new PreviewConfig(enablePreview, showAnnotations, showConfidence);
        }

        public Configuration(DarknetConfig darknetConfig, PreviewConfig previewConfig, bool enableVerboseLogging, bool saveInvalidData, double minimumConfidence)
        {
            DarknetConfig = darknetConfig ?? throw new ArgumentNullException(nameof(darknetConfig));
            PreviewConfig = previewConfig ?? throw new ArgumentNullException(nameof(previewConfig));
            EnableVerboseLogging = enableVerboseLogging;
            SaveInvalidData = saveInvalidData;
            MinimumConfidence = minimumConfidence;
        }
    }
}